using Core.Application.Authorization;
using Core.Application.Common.Interfaces;
using Core.Domain;
using Core.Infrastructure.Identity;
using Core.Infrastructure.Persistence;
using Core.Infrastructure.Persistence.Repositories;
using Core.Infrastructure.Persistence.Seeding;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Core.Tests.Infrastructure.Persistence;

public class DevelopmentAdminUserSeederTests
{
    [Fact]
    public async Task SeedAsync_Creates_Admin_User_Once()
    {
        await using var provider = await CreateMigratedServiceProviderAsync();
        using var scope = provider.CreateScope();

        var identitySeeder = scope.ServiceProvider.GetRequiredService<IIdentityDataSeeder>();
        await identitySeeder.SeedAsync();

        var seeder = scope.ServiceProvider.GetRequiredService<IDevelopmentAdminUserSeeder>();
        await seeder.SeedAsync();
        await seeder.SeedAsync();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var user = await userManager.FindByEmailAsync(DevelopmentAdminUserSeeder.DevAdminEmail);

        user.Should().NotBeNull();
        user!.TenantId.Should().Be(DevelopmentAdminUserSeeder.DevTenantId);
        user.AccessProfileId.Should().NotBe(Guid.Empty);
        (await userManager.IsInRoleAsync(user, ApplicationRoles.Admin)).Should().BeTrue();
    }

    private static async Task<ServiceProvider> CreateMigratedServiceProviderAsync()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped<ITenantContext>(_ => new TestTenantContext());
        services.AddDbContext<CoreDbContext>((_, options) => options.UseSqlite(connection));
        services.AddIdentity<AppUser, IdentityRole>()
            .AddEntityFrameworkStores<CoreDbContext>()
            .AddDefaultTokenProviders();
        services.AddScoped<IAccessProfileRepository, AccessProfileRepository>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<CoreDbContext>());
        services.AddScoped<IAccessProfileSeeder, AccessProfileSeeder>();
        services.AddScoped<IIdentityDataSeeder, IdentityDataSeeder>();
        services.AddScoped<IDevelopmentAdminUserSeeder, DevelopmentAdminUserSeeder>();

        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CoreDbContext>();
        await context.Database.MigrateAsync();

        return provider;
    }
}
