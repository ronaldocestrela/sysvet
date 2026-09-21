using Core.Domain;
using Core.Infrastructure.Configuration;
using Core.Application.Sync;
using Finance.Application;
using Finance.Application.Titles.Commands;
using Finance.Domain.Repositories;
using Finance.Infrastructure.Configuration;
using Finance.Infrastructure.Persistence;
using Finance.Infrastructure.Persistence.Repositories;
using Finance.Application.Reports;
using Finance.Infrastructure.Reports;
using Finance.Infrastructure.Sync;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Finance.Infrastructure;

/// <summary>
/// Registers Finance module persistence and MediatR handlers.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Finance DbContext, unit of work, validators, and command/query handlers.
    /// </summary>
    public static IServiceCollection AddFinanceModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatedOptions<FinanceOptions>(configuration, FinanceOptions.SectionName);

        services.AddDbContext<FinanceDbContext>((serviceProvider, options) =>
        {
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            var databaseOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            var moduleOptions = serviceProvider.GetRequiredService<IOptions<FinanceOptions>>().Value;
            options.ConfigureModuleDatabase(config, databaseOptions, moduleOptions.ConnectionString);
            options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        services.AddScoped<IFinancialTitleRepository, FinancialTitleRepository>();
        services.AddScoped<IFinancialCategoryRepository, FinancialCategoryRepository>();
        services.AddScoped<ICostCenterRepository, CostCenterRepository>();
        services.AddScoped<ICardReconciliationRepository, CardReconciliationRepository>();
        services.AddScoped<IFinanceStatementPdfRenderer, QuestPdfFinanceStatementRenderer>();

        services.AddScoped<IFinanceUnitOfWork>(provider => provider.GetRequiredService<FinanceDbContext>());
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<FinanceDbContext>());
        services.AddScoped<IDomainEventSource>(provider => provider.GetRequiredService<FinanceDbContext>());

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(CreateManualFinancialTitleCommand).Assembly));

        services.AddValidatorsFromAssembly(typeof(CreateManualFinancialTitleCommand).Assembly);

        services.AddScoped<ISyncPushHandler, FinanceSyncPushHandler>();
        services.AddScoped<ISyncChangeFeedContributor, FinanceSyncChangeFeedContributor>();

        return services;
    }
}
