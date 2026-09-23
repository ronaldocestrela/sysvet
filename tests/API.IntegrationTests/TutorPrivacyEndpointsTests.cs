using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Core.Application.Authorization;
using Core.Application.Common.Interfaces;
using Core.Application.Privacy;
using Core.Application.Tutors.Commands;
using Core.Infrastructure.Identity;
using Core.Infrastructure.Persistence;
using Core.Infrastructure.Persistence.Seeding;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace API.IntegrationTests;

[Collection("IntegrationTests")]
public class TutorPrivacyEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Password = "Password123!";
    private readonly WebApplicationFactory<Program> _factory;

    public TutorPrivacyEndpointsTests(WebApplicationFactory<Program> factory) => _factory = factory;

    private async Task<HttpClient> CreateAdminClientAsync()
    {
        using var scope = _factory.Services.CreateScope();
        await PrepareDatabasesAsync(scope);

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        const string email = "admin-privacy@sysvet.com";
        var user = new AppUser
        {
            UserName = email,
            Email = email,
            TenantId = IntegrationTestDatabaseHelper.SingleTenantId
        };
        await userManager.CreateAsync(user, Password);
        await userManager.AddToRoleAsync(user, ApplicationRoles.Admin);

        var client = _factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/v1/auth/login", new { Email = email, Password });
        var body = await login.Content.ReadFromJsonAsync<LoginResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body!.AccessToken);
        return client;
    }

    private static async Task PrepareDatabasesAsync(IServiceScope scope)
    {
        var coreContext = scope.ServiceProvider.GetRequiredService<CoreDbContext>();
        await coreContext.Database.EnsureDeletedAsync();
        await coreContext.Database.EnsureCreatedAsync();
        await IntegrationTestDatabaseHelper.MigrateModuleDatabasesAsync(scope);

        await scope.ServiceProvider.GetRequiredService<IAccessProfileSeeder>()
            .EnsureTenantProfilesAsync(IntegrationTestDatabaseHelper.SingleTenantId, CancellationToken.None);

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in ApplicationRoles.AllIncludingTutor)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    [Fact]
    public async Task AnonymizeTutor_ThenGet_ReturnsNotFound_AndAllowsCpfReuse()
    {
        var client = await CreateAdminClientAsync();
        var tutorId = Guid.NewGuid();
        var cpf = "63683891416";
        var create = await client.PostAsJsonAsync("/api/v1/tutors", new CreateTutorCommand(tutorId, "John Doe", "john@doe.com", cpf, "11999999999"));
        create.EnsureSuccessStatusCode();

        var erase = await client.DeleteAsync($"/api/v1/privacy/tutors/{tutorId}");
        erase.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var get = await client.GetAsync($"/api/v1/tutors/{tutorId}");
        get.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var recreate = await client.PostAsJsonAsync("/api/v1/tutors", new CreateTutorCommand(Guid.NewGuid(), "New John", "newjohn@doe.com", cpf, "11888888888"));
        recreate.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task ExportTutorPersonalData_ReturnsPackage()
    {
        var client = await CreateAdminClientAsync();
        var tutorId = Guid.NewGuid();
        var create = await client.PostAsJsonAsync("/api/v1/tutors", new CreateTutorCommand(tutorId, "Jane", "jane@doe.com", "10125103360", "11977777777"));
        create.EnsureSuccessStatusCode();

        var response = await client.GetAsync($"/api/v1/privacy/tutors/{tutorId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var package = await response.Content.ReadFromJsonAsync<TutorPersonalDataExportDto>();
        package!.TutorId.Should().Be(tutorId);
        package.Tutor.Email.Should().Be("jane@doe.com");
    }

    private sealed record LoginResponse(string AccessToken, string RefreshToken);
}
