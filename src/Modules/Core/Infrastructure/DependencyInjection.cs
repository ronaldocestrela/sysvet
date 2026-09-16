using Core.Application.Pets.Commands;
using Core.Domain;
using Core.Domain.Auditing;
using Core.Infrastructure.Auditing;
using Core.Infrastructure.Configuration;
using Core.Infrastructure.HealthChecks;
using Core.Infrastructure.Identity;
using Core.Infrastructure.Persistence;
using Core.Infrastructure.Persistence.Repositories;
using Core.Infrastructure.Services;
using Core.Infrastructure.Tenancy;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

        services.AddDbContext<CoreDbContext>((serviceProvider, options) =>
        {
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            var databaseOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            options.ConfigureModuleDatabase(config, databaseOptions);
            options.ReplaceService<Microsoft.EntityFrameworkCore.Infrastructure.IModelCacheKeyFactory, TenantAwareModelCacheKeyFactory>();
        });

        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("core-db", failureStatus: HealthStatus.Unhealthy, tags: ["ready", "db"]);

        services.AddIdentity<AppUser, IdentityRole>()
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
                policy.RequireRole("Admin", "Veterinarian", "Receptionist"));
            options.AddPolicy(Core.Application.Authorization.AuthorizationPolicies.Admin, policy => policy.RequireRole("Admin"));
            options.AddPolicy(Core.Application.Authorization.AuthorizationPolicies.Veterinarian, policy => policy.RequireRole("Veterinarian", "Admin"));
            options.AddPolicy(Core.Application.Authorization.AuthorizationPolicies.Receptionist, policy => policy.RequireRole("Receptionist", "Admin"));
            options.AddPolicy(Core.Application.Authorization.AuthorizationPolicies.Cashier, policy => policy.RequireRole("Cashier", "Admin"));
        });

        services.AddHttpContextAccessor();
        services.AddScoped<Core.Application.Common.Interfaces.ICurrentUser, HttpCurrentUser>();
        services.AddScoped<Core.Application.Common.Interfaces.IDomainEventDispatcher, MediatRDomainEventDispatcher>();

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ITutorRepository, TutorRepository>();
        services.AddScoped<IPetRepository, PetRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<CoreDbContext>());
        services.AddScoped<IDomainEventSource>(provider => provider.GetRequiredService<CoreDbContext>());
        services.AddScoped<IAuditLogger, AuditLogger>();
        services.AddScoped<Core.Application.Common.Interfaces.IIdempotencyService, IdempotencyService>();
        services.AddScoped<ITenantContext, DefaultTenantContext>();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(CreatePetCommand).Assembly);
            cfg.AddOpenBehavior(typeof(Core.Application.Behaviors.LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(Core.Application.Behaviors.AuthorizationBehavior<,>));
            cfg.AddOpenBehavior(typeof(Core.Application.Behaviors.ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(Core.Application.Behaviors.IdempotencyBehavior<,>));
            cfg.AddOpenBehavior(typeof(Core.Application.Behaviors.TransactionBehavior<,>));
        });

        FluentValidation.ServiceCollectionExtensions.AddValidatorsFromAssembly(services, typeof(CreatePetCommand).Assembly);

        return services;
    }
}
