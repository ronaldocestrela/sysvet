using Commerce.Application.Commands;
using Commerce.Application.Marketplace;
using Commerce.Domain.Repositories;
using Commerce.Infrastructure.Configuration;
using Commerce.Infrastructure.Marketplace;
using Commerce.Infrastructure.Persistence;
using Commerce.Infrastructure.Persistence.Repositories;
using Commerce.Infrastructure.Workers;
using Core.Domain;
using Core.Infrastructure.Configuration;
using Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Commerce.Infrastructure;

/// <summary>Registers Commerce module services.</summary>
public static class DependencyInjection
{
    /// <summary>Adds Commerce persistence, marketplace adapters and handlers.</summary>
    public static IServiceCollection AddCommerceModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatedOptions<CommerceOptions>(configuration, CommerceOptions.SectionName);

        services.AddDbContext<CommerceDbContext>((serviceProvider, options) =>
        {
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            var databaseOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            options.ConfigureModuleDatabase(config, databaseOptions);
            options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        services.AddScoped<IProductOfferRepository, ProductOfferRepository>();
        services.AddScoped<IOnlineOrderRepository, OnlineOrderRepository>();
        services.AddScoped<IMarketplaceSyncJobRepository, MarketplaceSyncJobRepository>();
        services.AddScoped<IMercadoLivreSettingsRepository, MercadoLivreSettingsRepository>();
        services.AddScoped<IMarketplaceSellerIndexRepository, MarketplaceSellerIndexRepository>();
        services.AddScoped<ICommerceUnitOfWork>(sp => sp.GetRequiredService<CommerceDbContext>());
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<CommerceDbContext>());
        services.AddScoped<IDomainEventSource>(sp => sp.GetRequiredService<CommerceDbContext>());

        services.AddScoped<MarketplaceSyncProcessor>();
        services.AddHostedService<MarketplaceSyncWorker>();

        var commerceOptions = configuration.GetSection(CommerceOptions.SectionName).Get<CommerceOptions>() ?? new CommerceOptions();
        if (commerceOptions.UseFakeMarketplaceChannel)
        {
            services.AddSingleton<FakeMarketplaceChannel>();
            services.AddSingleton<IMarketplaceChannel>(sp => sp.GetRequiredService<FakeMarketplaceChannel>());
        }
        else
        {
            services.AddHttpClient<MercadoLivreChannel>();
            services.AddScoped<IMarketplaceChannel, MercadoLivreChannel>();
        }

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ListProductOffersQuery).Assembly));

        return services;
    }
}
