using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace API.IntegrationTests;

public class AuthProductionEndpointsTests
{
    [Fact]
    public async Task Register_IsNotExposed_InProduction()
    {
        await using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment(Environments.Production);
                builder.ConfigureAppConfiguration((_, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["JwtSettings:Secret"] = "this-is-a-super-secret-key-that-needs-to-be-long-enough-for-hs256",
                        ["JwtSettings:Issuer"] = "sysvet-api",
                        ["JwtSettings:Audience"] = "sysvet-clients",
                        ["JwtSettings:ExpiryMinutes"] = "60",
                        ["JwtSettings:RefreshExpiryDays"] = "7",
                        ["ConnectionStrings:DefaultConnection"] = "Data Source=:memory:",
                        ["Database:Provider"] = "Sqlite",
                        ["TenancySettings:DefaultSchema"] = "dbo"
                    });
                });
            });

        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            Email = "blocked@sysvet.com",
            Password = "Password123!",
            Role = "Admin"
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
