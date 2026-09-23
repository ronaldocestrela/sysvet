using Core.Application.Common.Interfaces;
using Core.Application.Entitlements;
using Core.Domain;
using Core.Infrastructure.Configuration;
using Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Platform.Application.Abstractions;
using Platform.Application.Provisioning;
using Platform.Application.Tenancy;
using Platform.Application.Tenants.Commands;
using Platform.Domain.Repositories;
using Platform.Application.Configuration;
using Platform.Infrastructure.Configuration;
using Platform.Infrastructure.Entitlements;
using Platform.Infrastructure.Persistence;
using Platform.Infrastructure.Persistence.Repositories;
using Platform.Infrastructure.Persistence.Seeding;
using Platform.Application.Billing;
using Platform.Infrastructure.Billing;
using Platform.Infrastructure.Dunning;
using Platform.Infrastructure.HostedServices;
using Platform.Infrastructure.Provisioning;
using Platform.Infrastructure.Tenancy;
using Platform.Infrastructure.Identity;
using Platform.Infrastructure.Nfse;
using Platform.Infrastructure.Auditing;
using Platform.Infrastructure.ApiKeys;
using Platform.Infrastructure.Health;
using Platform.Infrastructure.Metrics;
using Platform.Application.Auditing;
using OpenAC.Net.NFSe.Nacional.Web;

namespace Platform.Infrastructure;

/// <summary>Registers Platform catalog services.</summary>
public static class DependencyInjection
{
    /// <summary>Adds Platform persistence and tenancy lookup services.</summary>
    public static IServiceCollection AddPlatformModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();
        services.AddValidatedOptions<PlatformOptions>(configuration, PlatformOptions.SectionName);
        services.AddValidatedOptions<BillingOptions>(configuration, BillingOptions.SectionName);
        services.AddValidatedOptions<DunningOptions>(configuration, DunningOptions.SectionName);
        services.AddValidatedOptions<NfseOptions>(configuration, NfseOptions.SectionName);
        services.AddValidatedOptions<ImpersonationOptions>(configuration, ImpersonationOptions.SectionName);

        services.AddDbContext<PlatformDbContext>((serviceProvider, options) =>
        {
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            var databaseOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            var moduleOptions = serviceProvider.GetRequiredService<IOptions<PlatformOptions>>().Value;
            options.ConfigureModuleDatabase(config, databaseOptions, moduleOptions.ConnectionString);
            options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IPlanRepository, PlanRepository>();
        services.AddScoped<IAddOnRepository, AddOnRepository>();
        services.AddScoped<ITenantSubscriptionRepository, TenantSubscriptionRepository>();
        services.AddScoped<IFeatureFlagRepository, FeatureFlagRepository>();
        services.AddScoped<ISubscriptionAdjustmentRepository, SubscriptionAdjustmentRepository>();
        services.AddScoped<IBillingCustomerRepository, BillingCustomerRepository>();
        services.AddScoped<IBillingPaymentMethodRepository, BillingPaymentMethodRepository>();
        services.AddScoped<IBillingInvoiceRepository, BillingInvoiceRepository>();
        services.AddScoped<IBillingWebhookReceiptRepository, BillingWebhookReceiptRepository>();
        services.AddScoped<IDunningNoticeRepository, DunningNoticeRepository>();
        services.AddScoped<ICouponRepository, CouponRepository>();
        services.AddScoped<ICouponRedemptionRepository, CouponRedemptionRepository>();
        services.AddScoped<ISaasServiceInvoiceRepository, SaasServiceInvoiceRepository>();
        services.AddScoped<IImpersonationSessionRepository, ImpersonationSessionRepository>();
        services.AddScoped<IImpersonationAuditRepository, ImpersonationAuditRepository>();
        services.AddScoped<IPlatformLoginLogRepository, PlatformLoginLogRepository>();
        services.AddScoped<IPlatformChangeAuditRepository, PlatformChangeAuditRepository>();
        services.AddScoped<IPartnerApiKeyRepository, PartnerApiKeyRepository>();
        services.AddScoped<ITenantRequestDailyRepository, TenantRequestDailyRepository>();
        services.AddScoped<IAcquisitionSpendRepository, AcquisitionSpendRepository>();
        services.AddScoped<ISaasMetricsReader, SaasMetricsReader>();
        services.AddScoped<IModuleAdoptionReader, ModuleAdoptionReader>();
        services.AddScoped<PlatformBackofficeAuditRecorder>();
        services.AddScoped<IGeoIpLookup, NullGeoIpLookup>();
        services.AddScoped<IPlatformAuditContext, HttpPlatformAuditContext>();
        services.AddScoped<IPlatformLoginUserResolver, PlatformLoginUserResolver>();
        services.AddSingleton<IPartnerApiKeySecretGenerator, PartnerApiKeySecretGenerator>();
        services.AddScoped<IPartnerRequestContext, PartnerRequestContext>();
        services.AddScoped<ITenantRequestRecorder, TenantRequestRecorder>();
        services.AddScoped<ITenantDataVolumeContributor, CoreTenantDataVolumeContributor>();
        services.AddScoped<IImpersonationAccessTokenIssuer, ImpersonationAccessTokenIssuer>();
        services.AddScoped<ITenantBillingStandingReader, TenantBillingStandingReader>();
        services.AddScoped<IBillingWebhookAuthenticator, BillingWebhookAuthenticator>();

        var billingProvider = configuration.GetSection(BillingOptions.SectionName).GetValue<string>(nameof(BillingOptions.Provider)) ?? "Fake";
        if (string.Equals(billingProvider, "Asaas", StringComparison.OrdinalIgnoreCase))
        {
            services.AddHttpClient<IBillingGateway, AsaasBillingGateway>();
        }
        else
        {
            services.AddSingleton<IBillingGateway, FakeBillingGateway>();
        }

        var dunningProvider = configuration.GetSection(DunningOptions.SectionName).GetValue<string>(nameof(DunningOptions.Provider)) ?? "Fake";
        if (string.Equals(dunningProvider, "Live", StringComparison.OrdinalIgnoreCase))
        {
            services.AddHttpClient("PlatformDunningSms", client =>
            {
                var baseUrl = configuration.GetSection(DunningOptions.SectionName).GetValue<string>(nameof(DunningOptions.SmsBaseUrl));
                if (!string.IsNullOrWhiteSpace(baseUrl))
                {
                    client.BaseAddress = new Uri(baseUrl);
                }
            });
            services.AddScoped<IDunningNotifier, LiveDunningNotifier>();
        }
        else
        {
            services.AddScoped<IDunningNotifier, FakeDunningNotifier>();
        }

        var nfseProvider = configuration.GetSection(NfseOptions.SectionName).GetValue<string>(nameof(NfseOptions.Provider)) ?? "Fake";
        if (string.Equals(nfseProvider, "OpenAc", StringComparison.OrdinalIgnoreCase))
        {
            services.AddOpenNFSeNacionalWebMultiTenant<PlatformSaasNfseConfigurationProvider, PlatformSaasNfseCertificateProvider>(infra =>
            {
                infra.PersistenciaHabilitada = false;
                infra.TimeoutOperacao = TimeSpan.FromSeconds(90);
            });
            services.AddScoped<ISaasNfseGateway, OpenAcSaasNfseGateway>();
        }
        else
        {
            services.AddSingleton<ISaasNfseGateway, FakeSaasNfseGateway>();
        }
        services.AddScoped<ITenantSubscriptionProvisioner, TenantSubscriptionProvisioner>();
        services.AddScoped<ITenantEntitlementReader, TenantEntitlementReader>();
        services.AddScoped<IBranchRepository, BranchRepository>();
        services.AddScoped<ITenantSlugLookup, TenantSlugLookup>();
        services.AddScoped<ITenantDirectory, TenantDirectory>();
        services.AddScoped<ITenantProvisioner, TenantProvisioner>();
        services.AddScoped<ITenantSignInGate, CatalogTenantSignInGate>();
        services.AddScoped<IPlatformUnitOfWork>(sp => sp.GetRequiredService<PlatformDbContext>());
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<PlatformDbContext>());

        services.AddScoped<IDevelopmentSuperAdminSeeder, DevelopmentSuperAdminSeeder>();
        services.AddHostedService<PlatformCatalogSeedHostedService>();
        services.AddHostedService<PlatformTenantSeedHostedService>();
        services.AddHostedService<DevelopmentSuperAdminSeedHostedService>();
        services.AddHostedService<TrialExpirationHostedService>();
        services.AddHostedService<BillingCycleHostedService>();
        services.AddHostedService<DunningHostedService>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(OnboardTenantCommand).Assembly));

        return services;
    }
}
