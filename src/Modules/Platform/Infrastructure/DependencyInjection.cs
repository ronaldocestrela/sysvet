using Core.Application.Common.Interfaces;
using Core.Application.Entitlements;
using Core.Domain;
using Core.Infrastructure.Configuration;
using Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Platform.Application.Abstractions;
using Platform.Application.Provisioning;
using Platform.Application.Tenancy;
using Platform.Application.Tenants.Commands;
using Platform.Domain.Repositories;
using Platform.Infrastructure.Configuration;
using Platform.Infrastructure.Entitlements;
using Platform.Infrastructure.Persistence;
using Platform.Infrastructure.Persistence.Repositories;
using Platform.Infrastructure.Persistence.Seeding;
using Platform.Infrastructure.HostedServices;
using Platform.Infrastructure.Provisioning;
using Platform.Infrastructure.Tenancy;

namespace Platform.Infrastructure;

/// <summary>Registers Platform catalog services.</summary>
public static class DependencyInjection
{
    /// <summary>Adds Platform persistence and tenancy lookup services.</summary>
    public static IServiceCollection AddPlatformModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();
        services.AddValidatedOptions<PlatformOptions>(configuration, PlatformOptions.SectionName);

        services.AddDbContext<PlatformDbContext>((serviceProvider, options) =>
        {
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            var databaseOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            var moduleOptions = serviceProvider.GetRequiredService<IOptions<PlatformOptions>>().Value;
            options.ConfigureModuleDatabase(config, databaseOptions, moduleOptions.ConnectionString);
            options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IPlanRepository, PlanRepository>();
        services.AddScoped<IAddOnRepository, AddOnRepository>();
        services.AddScoped<ITenantSubscriptionRepository, TenantSubscriptionRepository>();
        services.AddScoped<IFeatureFlagRepository, FeatureFlagRepository>();
        services.AddScoped<ISubscriptionAdjustmentRepository, SubscriptionAdjustmentRepository>();
        services.AddScoped<ITenantSubscriptionProvisioner, TenantSubscriptionProvisioner>();
        services.AddScoped<ITenantEntitlementReader, TenantEntitlementReader>();
        services.AddScoped<IBranchRepository, BranchRepository>();
        services.AddScoped<ITenantSlugLookup, TenantSlugLookup>();
        services.AddScoped<ITenantDirectory, TenantDirectory>();
        services.AddScoped<ITenantProvisioner, TenantProvisioner>();
        services.AddScoped<ITenantSignInGate, CatalogTenantSignInGate>();
        services.AddScoped<IPlatformUnitOfWork>(sp => sp.GetRequiredService<PlatformDbContext>());
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<PlatformDbContext>());

        services.AddScoped<IDevelopmentSuperAdminSeeder, DevelopmentSuperAdminSeeder>();
        services.AddHostedService<PlatformCatalogSeedHostedService>();
        services.AddHostedService<PlatformTenantSeedHostedService>();
        services.AddHostedService<DevelopmentSuperAdminSeedHostedService>();
        services.AddHostedService<TrialExpirationHostedService>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(OnboardTenantCommand).Assembly));

        return services;
    }
}
