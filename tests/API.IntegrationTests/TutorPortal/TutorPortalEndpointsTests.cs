using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Core.Application.Authorization;
using Core.Domain.Entities;
using Core.Domain.ValueObjects;
using Core.Infrastructure.Identity;
using Core.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TutorPortal.Infrastructure.Persistence;

namespace API.IntegrationTests.TutorPortal;

[Collection("IntegrationTests")]
public class TutorPortalEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string TutorEmail = "tutor.portal@test.com";
    private const string TutorCpf = "52998224725";
    private const string TutorPassword = "Password123!";

    private readonly WebApplicationFactory<Program> _factory;

    public TutorPortalEndpointsTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task Tutor_AuthenticatesSeparatelyFromClinicStaff()
    {
        await SeedCrmTutorAsync();
        var client = _factory.CreateClient();

        var registerResponse = await client.PostAsJsonAsync(
            "/api/v1/tutor-portal/register",
            new { Email = TutorEmail, Cpf = TutorCpf, Password = TutorPassword });
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginResponse = await client.PostAsJsonAsync(
            "/api/v1/tutor-portal/login",
            new { Email = TutorEmail, Password = TutorPassword });
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var staffLogin = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { Email = TutorEmail, Password = TutorPassword });
        staffLogin.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TutorSelfRegister_LinksExistingCrmTutor_WhenEmailAndCpfMatch()
    {
        var tutorId = await SeedCrmTutorAsync();
        var client = _factory.CreateClient();

        var registerResponse = await client.PostAsJsonAsync(
            "/api/v1/tutor-portal/register",
            new { Email = TutorEmail, Cpf = TutorCpf, Password = TutorPassword });
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var tokens = await registerResponse.Content.ReadFromJsonAsync<AuthTokensResponse>();
        tokens!.AccessToken.Should().NotBeNullOrEmpty();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        var meResponse = await client.GetAsync("/api/v1/tutor-portal/me");
        meResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var me = await meResponse.Content.ReadFromJsonAsync<TutorMeResponse>();
        me!.TutorId.Should().Be(tutorId);
    }

    [Fact]
    public async Task TutorToken_ForbiddenOnClinicTutorsApi()
    {
        await SeedCrmTutorAsync();
        var client = await CreateTutorClientAsync();

        var response = await client.GetAsync("/api/v1/tutors?pageNumber=1&pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ClinicStaffToken_ForbiddenOnTutorPortalMe()
    {
        var client = await CreateStaffClientAsync();

        var response = await client.GetAsync("/api/v1/tutor-portal/me");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private async Task<Guid> SeedCrmTutorAsync()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        await PrepareDatabasesAsync(scope);

        var core = scope.ServiceProvider.GetRequiredService<CoreDbContext>();
        var existing = await core.Tutors.FirstOrDefaultAsync(t => t.Email.Address == TutorEmail);
        if (existing is not null)
        {
            return existing.Id;
        }

        var tutor = Tutor.Create(
            "Portal Tutor",
            Email.Create(TutorEmail).Value,
            Cpf.Create(TutorCpf).Value,
            Phone.Create("11999997777").Value).Value;
        core.Tutors.Add(tutor);
        await core.SaveChangesAsync();
        return tutor.Id;
    }

    private async Task<HttpClient> CreateTutorClientAsync()
    {
        await SeedCrmTutorAsync();
        var client = _factory.CreateClient();
        var registerResponse = await client.PostAsJsonAsync(
            "/api/v1/tutor-portal/register",
            new { Email = TutorEmail, Cpf = TutorCpf, Password = TutorPassword });
        if (registerResponse.StatusCode == HttpStatusCode.OK)
        {
            var tokens = await registerResponse.Content.ReadFromJsonAsync<AuthTokensResponse>();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens!.AccessToken);
            return client;
        }

        var loginResponse = await client.PostAsJsonAsync(
            "/api/v1/tutor-portal/login",
            new { Email = TutorEmail, Password = TutorPassword });
        var loginTokens = await loginResponse.Content.ReadFromJsonAsync<AuthTokensResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginTokens!.AccessToken);
        return client;
    }

    private async Task<HttpClient> CreateStaffClientAsync()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        await PrepareDatabasesAsync(scope);

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in ApplicationRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var email = $"staff-{Guid.NewGuid():N}@sysvet.com";
        var user = new AppUser
        {
            UserName = email,
            Email = email,
            TenantId = IntegrationTestDatabaseHelper.SingleTenantId
        };
        await userManager.CreateAsync(user, TutorPassword);
        await userManager.AddToRoleAsync(user, ApplicationRoles.Admin);

        var client = _factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/v1/auth/login", new { Email = email, Password = TutorPassword });
        var tokens = await login.Content.ReadFromJsonAsync<AuthTokensResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens!.AccessToken);
        return client;
    }

    private static async Task PrepareDatabasesAsync(IServiceScope scope)
    {
        var coreContext = scope.ServiceProvider.GetRequiredService<CoreDbContext>();
        await coreContext.Database.EnsureDeletedAsync();
        await coreContext.Database.MigrateAsync();
        await IntegrationTestDatabaseHelper.MigrateModuleDatabasesAsync(scope);

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in ApplicationRoles.AllIncludingTutor)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private sealed record AuthTokensResponse(string AccessToken, string RefreshToken, int ExpiresInSeconds);

    private sealed record TutorMeResponse(Guid TutorId, string Name);
}
