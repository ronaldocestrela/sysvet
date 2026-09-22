using Core.Domain;
using Core.Infrastructure.Configuration;
using Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Platform.Application.Tenancy;
using Platform.Domain.Repositories;
using Platform.Infrastructure.Configuration;
using Platform.Infrastructure.Persistence;
using Platform.Infrastructure.Persistence.Repositories;
using Platform.Infrastructure.Persistence.Seeding;
using Platform.Infrastructure.Tenancy;

namespace Platform.Infrastructure;

/// <summary>Registers Platform catalog services.</summary>
public static class DependencyInjection
{
    /// <summary>Adds Platform persistence and tenancy lookup services.</summary>
    public static IServiceCollection AddPlatformModule(this IServiceCollection services, IConfiguration configuration)
    {
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
        services.AddScoped<ITenantSlugLookup, TenantSlugLookup>();
        services.AddScoped<ITenantDirectory, TenantDirectory>();
        services.AddScoped<IPlatformUnitOfWork>(sp => sp.GetRequiredService<PlatformDbContext>());
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<PlatformDbContext>());

        services.AddHostedService<PlatformTenantSeedHostedService>();

        return services;
    }
}
