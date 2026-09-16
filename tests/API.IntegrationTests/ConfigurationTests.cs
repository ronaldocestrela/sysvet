using System.Net;
using Core.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace API.IntegrationTests;

public class ConfigurationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ConfigurationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task DevelopmentHost_ShouldStart_WithDocumentedMinimalConfiguration()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public void ProductionHost_ShouldFailFast_WhenJwtSecretMissing()
    {
        var act = () =>
        {
            using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.UseSetting(WebHostDefaults.EnvironmentKey, Environments.Production);
                builder.ConfigureAppConfiguration((_, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Database:Provider"] = "SqlServer",
                        ["Database:ConnectionStringName"] = "DefaultConnection",
                        ["TenancySettings:DefaultSchema"] = "dbo",
                        ["JwtSettings:Issuer"] = "SysVet_Production",
                        ["JwtSettings:Audience"] = "SysVet_Clients",
                        ["JwtSettings:ExpiryMinutes"] = "60"
                    });
                });
            });

            factory.CreateClient();
        };

        act.Should().Throw<Exception>();
    }

    [Fact]
    public void Host_ShouldUseConfiguredDefaultConnectionString()
    {
        const string expectedFragment = "config-test-1.4.db";

        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = $"Data Source={expectedFragment}",
                    ["Database:Provider"] = "Sqlite",
                    ["Database:ConnectionStringName"] = "DefaultConnection",
                    ["TenancySettings:DefaultSchema"] = "dbo",
                    ["JwtSettings:Secret"] = "integration-test-secret-min-16",
                    ["JwtSettings:Issuer"] = "sysvet-api",
                    ["JwtSettings:Audience"] = "sysvet-clients",
                    ["JwtSettings:ExpiryMinutes"] = "60"
                });
            });
        });

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CoreDbContext>();
        var connectionString = dbContext.Database.GetDbConnection().ConnectionString;

        connectionString.Should().Contain(expectedFragment);
    }
}
