using Core.Application.Authorization;
using Core.Domain;
using Core.Infrastructure.Identity;
using Core.Infrastructure.Persistence;
using Core.Infrastructure.Persistence.Seeding;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Tests.Infrastructure.Persistence;

public class IdentityRoleSeedTests
{
    [Fact]
    public async Task SeedAsync_Should_CreateAllApplicationRoles()
    {
        await using var provider = await CreateMigratedServiceProviderAsync();
        using var scope = provider.CreateScope();

        var seeder = scope.ServiceProvider.GetRequiredService<IIdentityDataSeeder>();
        await seeder.SeedAsync(CancellationToken.None);

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in ApplicationRoles.All)
        {
            (await roleManager.RoleExistsAsync(role)).Should().BeTrue($"role {role} should exist");
        }
    }

    [Fact]
    public async Task SeedAsync_WhenCalledTwice_Should_BeIdempotent()
    {
        await using var provider = await CreateMigratedServiceProviderAsync();
        using var scope = provider.CreateScope();

        var seeder = scope.ServiceProvider.GetRequiredService<IIdentityDataSeeder>();
        await seeder.SeedAsync(CancellationToken.None);
        await seeder.SeedAsync(CancellationToken.None);

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var roles = await roleManager.Roles.Select(r => r.Name).ToListAsync();
        roles.Should().OnlyHaveUniqueItems();
        roles.Should().BeEquivalentTo(ApplicationRoles.AllIncludingTutor);
    }

    private static async Task<ServiceProvider> CreateMigratedServiceProviderAsync()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped<ITenantContext>(_ => new TestTenantContext());
        services.AddDbContext<CoreDbContext>((sp, options) => options.UseSqlite(connection));
        services.AddIdentity<AppUser, IdentityRole>()
            .AddEntityFrameworkStores<CoreDbContext>()
            .AddDefaultTokenProviders();
        services.AddScoped<IIdentityDataSeeder, IdentityDataSeeder>();

        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CoreDbContext>();
        await context.Database.MigrateAsync();

        return provider;
    }
}
