using Core.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace API.Extensions;

/// <summary>
/// Extensões para encapsular as configurações de Injeção de Dependência da API.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configura a documentação OpenAPI nativa do .NET 10.
    /// </summary>
    /// <param name="services">A coleção de serviços.</param>
    /// <returns>A própria coleção de serviços configurada.</returns>
    public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        services.AddOpenApi();
        return services;
    }

    /// <summary>
    /// Registra os serviços, handlers e infraestrutura do módulo Core.
    /// </summary>
    public static IServiceCollection AddCoreModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<Core.Infrastructure.Persistence.CoreDbContext>(options =>
        {
            options.UseSqlite("Data Source=sysvet.db");
            options.ReplaceService<Microsoft.EntityFrameworkCore.Infrastructure.IModelCacheKeyFactory, Core.Infrastructure.Persistence.TenantAwareModelCacheKeyFactory>();
        });

        // Identity
        services.AddIdentity<AppUser, IdentityRole>()
            .AddEntityFrameworkStores<Core.Infrastructure.Persistence.CoreDbContext>()
            .AddDefaultTokenProviders();

        // JWT Configuration
        var jwtSettingsSection = configuration.GetSection(JwtSettings.SectionName);
        var jwtSettings = jwtSettingsSection.Get<JwtSettings>();
        
        if (jwtSettings == null || string.IsNullOrWhiteSpace(jwtSettings.Secret))
        {
            // Default settings for testing if not present in appsettings
            jwtSettings = new JwtSettings { Secret = "super_secret_key_12345_for_testing_purposes_only!", Issuer = "sysvet", Audience = "sysvet", ExpiryMinutes = 60 };
        }

        services.AddOptions<JwtSettings>()
            .Bind(jwtSettingsSection)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<Core.Infrastructure.Tenancy.TenancySettings>()
            .Bind(configuration.GetSection(Core.Infrastructure.Tenancy.TenancySettings.SectionName))
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
        services.AddScoped<Core.Domain.ITutorRepository, Core.Infrastructure.Persistence.Repositories.TutorRepository>();
        services.AddScoped<Core.Domain.IPetRepository, Core.Infrastructure.Persistence.Repositories.PetRepository>();
        services.AddScoped<Core.Domain.IUnitOfWork>(provider => provider.GetRequiredService<Core.Infrastructure.Persistence.CoreDbContext>());
        
        services.AddScoped<Core.Domain.Auditing.IAuditLogger, Core.Infrastructure.Auditing.AuditLogger>();
        services.AddScoped<Core.Application.Common.Interfaces.IIdempotencyService, Core.Infrastructure.Services.IdempotencyService>();

        // Register default TenantContext for migrations/startup
        services.AddScoped<Core.Domain.ITenantContext, API.Services.DefaultTenantContext>();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(Core.Application.Pets.Commands.CreatePetCommand).Assembly);
            cfg.AddOpenBehavior(typeof(Core.Application.Behaviors.LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(Core.Application.Behaviors.ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(Core.Application.Behaviors.IdempotencyBehavior<,>));

            cfg.AddOpenBehavior(typeof(Core.Application.Behaviors.TransactionBehavior<,>));
        });

        FluentValidation.ServiceCollectionExtensions.AddValidatorsFromAssembly(services, typeof(Core.Application.Pets.Commands.CreatePetCommand).Assembly);

        return services;
    }
    public static IServiceCollection AddVeterinaryModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<Veterinary.Infrastructure.Persistence.VeterinaryDbContext>(options =>
        {
            options.UseSqlite("Data Source=sysvet.db"); // Utilizando o mesmo BD do Core para manter simples na PoC
        });

        services.AddScoped<Veterinary.Domain.Repositories.IAppointmentRepository, Veterinary.Infrastructure.Persistence.Repositories.AppointmentRepository>();
        services.AddScoped<Veterinary.Domain.Repositories.IScheduleSlotRepository, Veterinary.Infrastructure.Persistence.Repositories.ScheduleSlotRepository>();
        services.AddScoped<Veterinary.Domain.Repositories.IUnitOfWork>(provider => provider.GetRequiredService<Veterinary.Infrastructure.Persistence.VeterinaryDbContext>());

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(Veterinary.Application.Appointments.Commands.ScheduleAppointmentCommand).Assembly);
        });

        FluentValidation.ServiceCollectionExtensions.AddValidatorsFromAssembly(services, typeof(Veterinary.Application.Appointments.Commands.ScheduleAppointmentCommand).Assembly);

        return services;
    }
}
