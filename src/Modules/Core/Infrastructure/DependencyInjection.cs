using Core.Application.Pets.Commands;
using Core.Domain;
using Core.Domain.Auditing;
using Core.Infrastructure.Auditing;
using Core.Infrastructure.Configuration;
using Core.Infrastructure.HealthChecks;
using Core.Infrastructure.Identity;
using Core.Application.Authorization;
using Core.Application.Common.Interfaces;
using Core.Application.Entitlements;
using Core.Infrastructure.Entitlements;
using Core.Application.Sync;
using Core.Infrastructure.Persistence;
using Core.Infrastructure.Persistence.Repositories;
using Core.Infrastructure.Persistence.Seeding;
using Core.Infrastructure.Services;
using Core.Application.Notifications;
using Core.Infrastructure.Notifications;
using Core.Infrastructure.Storage;
using Core.Infrastructure.Tenancy;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Core.Infrastructure;

/// <summary>
/// Registers Core module infrastructure, application handlers, and cross-cutting MediatR behaviors.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Core persistence, identity, authorization, repositories, and CQRS pipeline behaviors.
    /// </summary>
    public static IServiceCollection AddCoreModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatedOptions<DatabaseOptions>(configuration, DatabaseOptions.SectionName);
        services.AddValidatedOptions<JwtSettings>(configuration, JwtSettings.SectionName);
        services.AddValidatedOptions<TenancySettings>(configuration, TenancySettings.SectionName);
        services.AddSysvetDistributedCache(configuration);
        services.AddBlobStorage(configuration);

        services.AddDbContext<CoreDbContext>((serviceProvider, options) =>
        {
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            var databaseOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            options.ConfigureModuleDatabase(config, databaseOptions);
            options.ReplaceService<Microsoft.EntityFrameworkCore.Infrastructure.IModelCacheKeyFactory, TenantAwareModelCacheKeyFactory>();
        });

        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("core-db", failureStatus: HealthStatus.Unhealthy, tags: ["ready", "db"]);

        services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.AllowedForNewUsers = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
            })
            .AddEntityFrameworkStores<CoreDbContext>()
            .AddDefaultTokenProviders();

        services.AddSingleton<IConfigureOptions<JwtBearerOptions>, JwtBearerOptionsConfiguration>();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer();

        services.AddAuthorization(options =>
        {
            options.AddPolicy(Core.Application.Authorization.AuthorizationPolicies.Authenticated, policy => policy.RequireAuthenticatedUser());
            options.AddPolicy(Core.Application.Authorization.AuthorizationPolicies.ClinicStaff, policy =>
                policy.RequireRole(ApplicationRoles.Admin, ApplicationRoles.Veterinarian, ApplicationRoles.Receptionist));
            options.AddPolicy(Core.Application.Authorization.AuthorizationPolicies.Admin, policy => policy.RequireRole(ApplicationRoles.Admin));
            options.AddPolicy(Core.Application.Authorization.AuthorizationPolicies.Veterinarian, policy => policy.RequireRole(ApplicationRoles.Veterinarian, ApplicationRoles.Admin));
            options.AddPolicy(Core.Application.Authorization.AuthorizationPolicies.Receptionist, policy => policy.RequireRole(ApplicationRoles.Receptionist, ApplicationRoles.Admin));
            options.AddPolicy(Core.Application.Authorization.AuthorizationPolicies.Cashier, policy => policy.RequireRole(ApplicationRoles.Cashier, ApplicationRoles.Admin));
            options.AddPolicy(Core.Application.Authorization.AuthorizationPolicies.ClinicUser, policy =>
                policy.RequireRole(
                    ApplicationRoles.Admin,
                    ApplicationRoles.Veterinarian,
                    ApplicationRoles.Receptionist,
                    ApplicationRoles.Cashier));
            options.AddPolicy(Core.Application.Authorization.AuthorizationPolicies.TutorPortal, policy =>
                policy.RequireRole(ApplicationRoles.Tutor));
            options.AddPolicy(Core.Application.Authorization.AuthorizationPolicies.PlatformAdmin, policy =>
                policy.RequireRole(ApplicationRoles.SuperAdmin));
        });

        services.AddHttpContextAccessor();
        services.AddScoped<Core.Application.Common.Interfaces.ICurrentUser, HttpCurrentUser>();
        services.AddScoped<Core.Application.Common.Interfaces.IDomainEventDispatcher, MediatRDomainEventDispatcher>();
        services.TryAddSingleton<ITutorNotificationChannel, NullTutorNotificationChannel>();
        services.TryAddSingleton<ISyncPushObserver, NullSyncPushObserver>();

        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IAccessTokenIssuer, JwtAccessTokenIssuer>();
        services.AddScoped<IRefreshTokenStore, RefreshTokenStore>();
        services.AddScoped<ITutorRepository, TutorRepository>();
        services.AddScoped<IPetRepository, PetRepository>();
        services.AddScoped<ISyncChangeFeedReader, SyncChangeFeedReader>();
        services.AddScoped<IAccessProfileRepository, AccessProfileRepository>();
        services.AddScoped<IUserPreferenceRepository, UserPreferenceRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IAccessProfileSeeder, AccessProfileSeeder>();
        services.AddScoped<IPermissionChecker, PermissionChecker>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<CoreDbContext>());
        services.AddScoped<IDomainEventSource>(provider => provider.GetRequiredService<CoreDbContext>());
        services.AddScoped<IAuditLogger, AuditLogger>();
        services.AddScoped<Core.Application.Common.Interfaces.IIdempotencyService, IdempotencyService>();
        services.AddScoped<ITenantContext, DefaultTenantContext>();
        services.AddScoped<IIdentityDataSeeder, IdentityDataSeeder>();
        services.AddScoped<IDevelopmentAdminUserSeeder, DevelopmentAdminUserSeeder>();
        services.AddHostedService<IdentityDataSeedHostedService>();
        services.AddHostedService<DevelopmentAdminUserSeedHostedService>();
        services.TryAddScoped<ITenantEntitlementReader, AllowAllEntitlementReader>();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(CreatePetCommand).Assembly);
            cfg.AddOpenBehavior(typeof(Core.Application.Behaviors.LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(Core.Application.Behaviors.AuthorizationBehavior<,>));
            cfg.AddOpenBehavior(typeof(Core.Application.Behaviors.ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(Core.Application.Behaviors.DistributedCacheBehavior<,>));
            cfg.AddOpenBehavior(typeof(Core.Application.Behaviors.IdempotencyBehavior<,>));
            cfg.AddOpenBehavior(typeof(Core.Application.Behaviors.TransactionBehavior<,>));
        });

        FluentValidation.ServiceCollectionExtensions.AddValidatorsFromAssembly(services, typeof(CreatePetCommand).Assembly);

        return services;
    }
}
