using Core.Application.Pets.Commands;
using Core.Domain;
using Core.Domain.Auditing;
using Core.Infrastructure.Auditing;
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
using Microsoft.IdentityModel.Tokens;
using System.Text;

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
        services.AddDbContext<CoreDbContext>(options =>
        {
            options.UseSqlite("Data Source=sysvet.db");
            options.ReplaceService<Microsoft.EntityFrameworkCore.Infrastructure.IModelCacheKeyFactory, TenantAwareModelCacheKeyFactory>();
        });

        services.AddIdentity<AppUser, IdentityRole>()
            .AddEntityFrameworkStores<CoreDbContext>()
            .AddDefaultTokenProviders();

        var jwtSettingsSection = configuration.GetSection(JwtSettings.SectionName);
        var jwtSettings = jwtSettingsSection.Get<JwtSettings>();

        if (jwtSettings == null || string.IsNullOrWhiteSpace(jwtSettings.Secret))
        {
            jwtSettings = new JwtSettings
            {
                Secret = "super_secret_key_12345_for_testing_purposes_only!",
                Issuer = "sysvet",
                Audience = "sysvet",
                ExpiryMinutes = 60
            };
        }

        services.AddOptions<JwtSettings>()
            .Bind(jwtSettingsSection)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<TenancySettings>()
            .Bind(configuration.GetSection(TenancySettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
            };
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
            options.AddPolicy("Veterinarian", policy => policy.RequireRole("Veterinarian", "Admin"));
            options.AddPolicy("Receptionist", policy => policy.RequireRole("Receptionist", "Admin"));
            options.AddPolicy("Cashier", policy => policy.RequireRole("Cashier", "Admin"));
        });

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ITutorRepository, TutorRepository>();
        services.AddScoped<IPetRepository, PetRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<CoreDbContext>());
        services.AddScoped<IAuditLogger, AuditLogger>();
        services.AddScoped<Core.Application.Common.Interfaces.IIdempotencyService, IdempotencyService>();
        services.AddScoped<ITenantContext, DefaultTenantContext>();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(CreatePetCommand).Assembly);
            cfg.AddOpenBehavior(typeof(Core.Application.Behaviors.LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(Core.Application.Behaviors.ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(Core.Application.Behaviors.IdempotencyBehavior<,>));
            cfg.AddOpenBehavior(typeof(Core.Application.Behaviors.TransactionBehavior<,>));
        });

        FluentValidation.ServiceCollectionExtensions.AddValidatorsFromAssembly(services, typeof(CreatePetCommand).Assembly);

        return services;
    }
}
