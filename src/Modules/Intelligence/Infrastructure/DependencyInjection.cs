using Core.Domain;
using Core.Infrastructure.Configuration;
using Intelligence.Application.Dashboard.Commands;
using Intelligence.Application.Reports;
using Intelligence.Domain.Repositories;
using Intelligence.Infrastructure.Configuration;
using Intelligence.Infrastructure.Persistence;
using Intelligence.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Intelligence.Infrastructure;

/// <summary>Registers Intelligence module services.</summary>
public static class DependencyInjection
{
    /// <summary>Adds Intelligence persistence and MediatR handlers.</summary>
    public static IServiceCollection AddIntelligenceModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatedOptions<IntelligenceOptions>(configuration, IntelligenceOptions.SectionName);

        services.AddDbContext<IntelligenceDbContext>((serviceProvider, options) =>
        {
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            var databaseOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            var moduleOptions = serviceProvider.GetRequiredService<IOptions<IntelligenceOptions>>().Value;
            options.ConfigureModuleDatabase(config, databaseOptions, moduleOptions.ConnectionString);
            options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        services.AddScoped<IntelligenceReportComposer>();
        services.AddScoped<IProfileDashboardLayoutRepository, ProfileDashboardLayoutRepository>();
        services.AddScoped<IIntelligenceUnitOfWork>(sp => sp.GetRequiredService<IntelligenceDbContext>());
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<IntelligenceDbContext>());
        services.AddScoped<IDomainEventSource>(sp => sp.GetRequiredService<IntelligenceDbContext>());

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(UpsertProfileDashboardLayoutCommand).Assembly));

        return services;
    }
}
