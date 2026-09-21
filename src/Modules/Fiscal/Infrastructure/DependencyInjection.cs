using Core.Domain;
using Core.Infrastructure.Configuration;
using Fiscal.Application.Abstractions;
using Fiscal.Application.Documents;
using Fiscal.Application.Issuer;
using Fiscal.Domain.Repositories;
using Fiscal.Infrastructure.Configuration;
using Fiscal.Infrastructure.Gateways;
using Fiscal.Infrastructure.OpenAc;
using Fiscal.Infrastructure.Persistence;
using Fiscal.Infrastructure.Persistence.Repositories;
using Fiscal.Infrastructure.Reports;
using Fiscal.Infrastructure.Security;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenAC.Net.NFSe.Nacional.Web;

namespace Fiscal.Infrastructure;

/// <summary>Registers Fiscal module services.</summary>
public static class DependencyInjection
{
    /// <summary>Adds Fiscal persistence, gateways, and MediatR handlers.</summary>
    public static IServiceCollection AddFiscalModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatedOptions<FiscalOptions>(configuration, FiscalOptions.SectionName);

        services.AddDbContext<FiscalDbContext>((serviceProvider, options) =>
        {
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            var databaseOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            var moduleOptions = serviceProvider.GetRequiredService<IOptions<FiscalOptions>>().Value;
            options.ConfigureModuleDatabase(config, databaseOptions, moduleOptions.ConnectionString);
            options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        services.AddScoped<IIssuerProfileRepository, IssuerProfileRepository>();
        services.AddScoped<IFiscalDocumentRepository, FiscalDocumentRepository>();
        services.AddScoped<IFiscalUnitOfWork>(sp => sp.GetRequiredService<FiscalDbContext>());
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<FiscalDbContext>());
        services.AddScoped<IDomainEventSource>(sp => sp.GetRequiredService<FiscalDbContext>());

        services.AddScoped<ICertificateProtector, AesCertificateProtector>();
        services.AddScoped<IDanfeRenderer, FakeDanfeRenderer>();
        services.AddScoped<INfceDanfeRenderer, FakeNfceDanfeRenderer>();

        var provider = configuration.GetSection(FiscalOptions.SectionName).GetValue<string>(nameof(FiscalOptions.Provider)) ?? "Fake";
        if (string.Equals(provider, "ZeusOpenAc", StringComparison.OrdinalIgnoreCase))
        {
            services.AddOpenNFSeNacionalWebMultiTenant<IssuerNfseConfigurationProvider, IssuerNfseCertificateProvider>(infra =>
            {
                infra.PersistenciaHabilitada = false;
                infra.TimeoutOperacao = TimeSpan.FromSeconds(90);
            });
            services.AddScoped<INfeGateway, ZeusNfeGateway>();
            services.AddScoped<INfseGateway, OpenAcNacionalWebNfseGateway>();
            services.AddScoped<INfceGateway, ZeusNfceGateway>();
        }
        else
        {
            services.AddScoped<INfeGateway, FakeNfeGateway>();
            services.AddScoped<INfseGateway, FakeNfseGateway>();
            services.AddScoped<INfceGateway, FakeNfceGateway>();
        }

        services.AddScoped<Core.Application.Sync.ISyncPushHandler, Sync.FiscalSyncPushHandler>();
        services.AddScoped<Core.Application.Sync.ISyncChangeFeedContributor, Sync.FiscalSyncChangeFeedContributor>();

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(IssueFromOrderCommand).Assembly));

        services.AddValidatorsFromAssembly(typeof(IssueFromOrderCommand).Assembly);

        return services;
    }
}
