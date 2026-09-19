using Core.Domain;
using Core.Infrastructure.Configuration;
using Inventory.Application.Common;
using Inventory.Application.Products.Commands;
using Inventory.Application.StockMovements.Commands;
using Inventory.Domain.Repositories;
using Inventory.Infrastructure.Configuration;
using Inventory.Infrastructure.Persistence;
using Inventory.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Inventory.Infrastructure;

/// <summary>
/// Registers Inventory module persistence, repositories, and MediatR handlers.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Inventory DbContext, repositories, validators, and command/query handlers.
    /// </summary>
    public static IServiceCollection AddInventoryModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatedOptions<InventoryOptions>(configuration, InventoryOptions.SectionName);

        services.AddDbContext<InventoryDbContext>((serviceProvider, options) =>
        {
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            var databaseOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            var moduleOptions = serviceProvider.GetRequiredService<IOptions<InventoryOptions>>().Value;
            options.ConfigureModuleDatabase(config, databaseOptions, moduleOptions.ConnectionString);
        });

        services.AddScoped<IInventoryUnitOfWork>(provider => provider.GetRequiredService<InventoryDbContext>());
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<InventoryDbContext>());
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IStockMovementRepository, StockMovementRepository>();
        services.AddScoped<ISupplierRepository, SupplierRepository>();
        services.AddScoped<IProductLotRepository, ProductLotRepository>();
        services.AddScoped<IPurchaseInvoiceImportRepository, PurchaseInvoiceImportRepository>();
        services.AddScoped<ISupplierProductMappingRepository, SupplierProductMappingRepository>();
        services.AddScoped<StockCatalogReconciler>();
        services.AddScoped<StockLedgerWriter>();
        services.AddScoped<TransferStockCommandHandler>();
        services.AddScoped<Core.Application.Sync.ISyncChangeFeedContributor, Sync.InventorySyncChangeFeedContributor>();
        services.AddScoped<Core.Application.Sync.ISyncPushHandler, Sync.InventorySyncPushHandler>();

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(RegisterProductCommand).Assembly));

        FluentValidation.ServiceCollectionExtensions.AddValidatorsFromAssembly(services, typeof(RegisterProductCommand).Assembly);

        return services;
    }
}
