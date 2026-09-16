using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sales.Application.Orders.Commands;
using Sales.Domain.Repositories;
using Sales.Infrastructure.Persistence;
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
        _ = configuration;

        services.AddDbContext<SalesDbContext>(options =>
            options.UseSqlite("Data Source=sysvet.db"));

        services.AddScoped<ISalesUnitOfWork>(provider => provider.GetRequiredService<SalesDbContext>());
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ICashRegisterRepository, CashRegisterRepository>();

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(CreateOrderCommand).Assembly));

        FluentValidation.ServiceCollectionExtensions.AddValidatorsFromAssembly(services, typeof(CreateOrderCommand).Assembly);

        return services;
    }
}
