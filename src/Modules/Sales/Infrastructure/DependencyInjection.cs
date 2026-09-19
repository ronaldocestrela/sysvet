using Core.Domain;
using Core.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Sales.Application.Orders.Commands;
using Sales.Domain.Repositories;
using Sales.Infrastructure.Configuration;
using Sales.Infrastructure.Persistence;
using Sales.Domain.Payments;
using Sales.Infrastructure.Persistence.Repositories;

namespace Sales.Infrastructure;

/// <summary>
/// Registers Sales module persistence, repositories, and MediatR handlers.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Sales DbContext, repositories, validators, and command/query handlers.
    /// </summary>
    public static IServiceCollection AddSalesModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatedOptions<SalesOptions>(configuration, SalesOptions.SectionName);

        services.AddDbContext<SalesDbContext>((serviceProvider, options) =>
        {
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            var databaseOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            var moduleOptions = serviceProvider.GetRequiredService<IOptions<SalesOptions>>().Value;
            options.ConfigureModuleDatabase(config, databaseOptions, moduleOptions.ConnectionString);
            options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        services.AddScoped<ISalesUnitOfWork>(provider => provider.GetRequiredService<SalesDbContext>());
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<SalesDbContext>());
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ICashRegisterRepository, CashRegisterRepository>();
        services.AddScoped<ICommissionRuleRepository, CommissionRuleRepository>();

        services.AddSingleton<IPaymentTerminal, SimulatedPaymentTerminal>();

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(CreateOrderCommand).Assembly));

        FluentValidation.ServiceCollectionExtensions.AddValidatorsFromAssembly(services, typeof(CreateOrderCommand).Assembly);

        services.AddScoped<Core.Application.Sync.ISyncPushHandler, Sync.SalesSyncPushHandler>();
        services.AddScoped<Core.Application.Sync.ISyncChangeFeedContributor, Sync.SalesSyncChangeFeedContributor>();

        return services;
    }
}
