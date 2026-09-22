using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ClinicSite.Application.Dtos;
using Core.Application.Authorization;
using Core.Infrastructure.Identity;
using Core.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace API.IntegrationTests.ClinicSite;

[Collection("IntegrationTests")]
public class ClinicSiteEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Password = "Password123!";

    private readonly WebApplicationFactory<Program> _factory;

    public ClinicSiteEndpointsTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task ClinicSite_PublishedWithCadastralData_WhenSlugResolved()
    {
        var staff = await CreateStaffClientAsync();
        const string slug = "clinica-pet-test";

        var putResponse = await staff.PutAsJsonAsync("/api/v1/clinic-site", new
        {
            DisplayName = "Clínica Pet Test",
            Tagline = "Cuidamos do seu pet",
            Street = "Rua das Flores",
            Number = "100",
            Complement = (string?)null,
            District = "Centro",
            City = "São Paulo",
            State = "SP",
            PostalCode = "01001000",
            Phone = "11988887777",
            Email = "contato@clinicapet.test",
            WhatsApp = (string?)null,
            LogoUrl = (string?)null,
            Slug = slug
        });
        putResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);

        await staff.PutAsJsonAsync("/api/v1/clinic-site/services", new
        {
            Services = new[]
            {
                new { Id = Guid.Empty, Name = "Consulta", Description = "Consulta clínica", DurationMinutes = 30, Price = (decimal?)150m, SortOrder = 0, IsVisible = true }
            }
        });

        await staff.PutAsJsonAsync("/api/v1/clinic-site/team", new
        {
            Team = new[]
            {
                new { Id = Guid.Empty, Name = "Dra. Ana", RoleTitle = "Veterinária", Bio = "CRMV 123", SortOrder = 0, IsVisible = true }
            }
        });

        await staff.PutAsJsonAsync("/api/v1/clinic-site/hours", new
        {
            Hours = new[]
            {
                new { Id = Guid.Empty, Day = DayOfWeek.Monday, OpenTime = "08:00", CloseTime = "18:00", IsClosed = false }
            }
        });

        var publish = await staff.PostAsync("/api/v1/clinic-site/publish", null);
        publish.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);

        var publicClient = _factory.CreateClient();
        var publicResponse = await publicClient.GetAsync($"/api/v1/public/clinic-sites/{slug}");
        publicResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await publicResponse.Content.ReadFromJsonAsync<PublicClinicSiteDto>();
        dto!.Slug.Should().Be(slug);
        dto.DisplayName.Should().Be("Clínica Pet Test");
        dto.Street.Should().Be("Rua das Flores");
        dto.Phone.Should().Be("11988887777");
        dto.Email.Should().Be("contato@clinicapet.test");
        dto.Services.Should().ContainSingle(s => s.Name == "Consulta");
        dto.Team.Should().ContainSingle(t => t.Name == "Dra. Ana");
        dto.Hours.Should().ContainSingle(h => h.Day == DayOfWeek.Monday);
    }

    [Fact]
    public async Task ClinicSite_NotFound_WhenUnpublishedOrUnknownSlug()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        await PrepareDatabasesAsync(scope);

        var publicClient = _factory.CreateClient();
        var unknown = await publicClient.GetAsync("/api/v1/public/clinic-sites/nao-existe-slug");
        unknown.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var staff = await CreateStaffClientAsync();
        await staff.PutAsJsonAsync("/api/v1/clinic-site", new
        {
            DisplayName = "Draft Clinic",
            Tagline = (string?)null,
            Street = "Rua B",
            Number = "1",
            Complement = (string?)null,
            District = "Centro",
            City = "São Paulo",
            State = "SP",
            PostalCode = "01001000",
            Phone = "11977776666",
            Email = "",
            WhatsApp = (string?)null,
            LogoUrl = (string?)null,
            Slug = "draft-clinic"
        });

        var draft = await publicClient.GetAsync("/api/v1/public/clinic-sites/draft-clinic");
        draft.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ClinicStaffToken_RequiredToPublishClinicSite()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsync("/api/v1/clinic-site/publish", null);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
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

        var email = $"staff-clinicsite-{Guid.NewGuid():N}@sysvet.com";
        var user = new AppUser
        {
            UserName = email,
            Email = email,
            TenantId = IntegrationTestDatabaseHelper.SingleTenantId
        };
        await userManager.CreateAsync(user, Password);
        await userManager.AddToRoleAsync(user, ApplicationRoles.Admin);

        var client = _factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/v1/auth/login", new { Email = email, Password = Password });
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
}
