using ClinicSite.Application.Commands;
using ClinicSite.Domain.Repositories;
using ClinicSite.Infrastructure.Configuration;
using ClinicSite.Infrastructure.Persistence;
using ClinicSite.Infrastructure.Persistence.Repositories;
using Core.Domain;
using Core.Infrastructure.Configuration;
using Core.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ClinicSite.Infrastructure;

/// <summary>
/// Registers ClinicSite module persistence and MediatR handlers.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds ClinicSite DbContext, repositories, and application handlers.
    /// </summary>
    public static IServiceCollection AddClinicSiteModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatedOptions<ClinicSiteOptions>(configuration, ClinicSiteOptions.SectionName);

        services.AddDbContext<ClinicSiteDbContext>((serviceProvider, options) =>
        {
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            var databaseOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            options.ConfigureModuleDatabase(config, databaseOptions);
            options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        services.AddScoped<IClinicSiteProfileRepository, ClinicSiteProfileRepository>();
        services.AddScoped<IClinicSiteSlugIndexRepository, ClinicSiteSlugIndexRepository>();
        services.AddScoped<IClinicSiteUnitOfWork>(sp => sp.GetRequiredService<ClinicSiteDbContext>());
        services.AddScoped<IDomainEventSource>(sp => sp.GetRequiredService<ClinicSiteDbContext>());

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetClinicSiteQuery).Assembly));

        return services;
    }
}
