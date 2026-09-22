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
using Xunit;

namespace API.IntegrationTests.TutorPortal;

[Collection("IntegrationTests")]
public class TutorPortalPushEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string TutorEmail = "tutor.push@test.com";
    private const string TutorCpf = "52998224725";
    private const string TutorPassword = "Password123!";

    private readonly WebApplicationFactory<Program> _factory;

    public TutorPortalPushEndpointsTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task TutorPush_SubscribePersists_WhenAuthenticated()
    {
        await SeedTutorAsync();
        var client = await CreateTutorClientAsync();

        var response = await client.PostAsJsonAsync("/api/v1/tutor-portal/push/subscribe", new
        {
            Endpoint = "https://push.test/endpoint-1",
            P256dh = "test-p256dh",
            Auth = "test-auth",
            UserAgent = "test-agent"
        });

        response.IsSuccessStatusCode.Should().BeTrue();

        await using var scope = _factory.Services.CreateAsyncScope();
        var portalContext = scope.ServiceProvider.GetRequiredService<TutorPortalDbContext>();
        var count = await portalContext.TutorPushSubscriptions.CountAsync();
        count.Should().Be(1);
    }

    private async Task SeedTutorAsync()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var core = scope.ServiceProvider.GetRequiredService<CoreDbContext>();
        await core.Database.EnsureDeletedAsync();
        await core.Database.MigrateAsync();
        await IntegrationTestDatabaseHelper.MigrateModuleDatabasesAsync(scope);

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in ApplicationRoles.AllIncludingTutor)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var tutor = Tutor.Create(
            "Push Tutor",
            Email.Create(TutorEmail).Value,
            Cpf.Create(TutorCpf).Value,
            Phone.Create("11999997777").Value).Value;
        core.Tutors.Add(tutor);
        await core.SaveChangesAsync();
    }

    private async Task<HttpClient> CreateTutorClientAsync()
    {
        var client = _factory.CreateClient();
        var registerResponse = await client.PostAsJsonAsync(
            "/api/v1/tutor-portal/register",
            new { Email = TutorEmail, Cpf = TutorCpf, Password = TutorPassword });
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var tokens = await registerResponse.Content.ReadFromJsonAsync<AuthTokensResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens!.AccessToken);
        return client;
    }

    private sealed record AuthTokensResponse(string AccessToken, string RefreshToken, int ExpiresInSeconds);
}
