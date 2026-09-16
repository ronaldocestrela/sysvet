using Inventory.Application.Products.Commands;
using Inventory.Domain.Repositories;
using Inventory.Infrastructure.Persistence;
using Inventory.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
        _ = configuration;

        services.AddDbContext<InventoryDbContext>(options =>
            options.UseSqlite("Data Source=sysvet.db"));

        services.AddScoped<IInventoryUnitOfWork>(provider => provider.GetRequiredService<InventoryDbContext>());
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IStockMovementRepository, StockMovementRepository>();

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(RegisterProductCommand).Assembly));

        FluentValidation.ServiceCollectionExtensions.AddValidatorsFromAssembly(services, typeof(RegisterProductCommand).Assembly);

        return services;
    }
}
