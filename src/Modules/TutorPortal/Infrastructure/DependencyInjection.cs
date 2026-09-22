using Core.Domain;
using Core.Infrastructure.Configuration;
using Core.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TutorPortal.Application.Abstractions;
using TutorPortal.Application.Auth;
using TutorPortal.Application.Auth.Commands;
using TutorPortal.Domain.Repositories;
using TutorPortal.Infrastructure.Configuration;
using TutorPortal.Application.PetHealth;
using TutorPortal.Infrastructure.Crm;
using TutorPortal.Infrastructure.PetHealth;
using TutorPortal.Infrastructure.Scheduling;
using TutorPortal.Infrastructure.Push;
using TutorPortal.Infrastructure.Persistence;
using TutorPortal.Infrastructure.Persistence.Repositories;

namespace TutorPortal.Infrastructure;

/// <summary>
/// Registers TutorPortal module persistence and MediatR handlers.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds TutorPortal DbContext, repositories, validators, and auth services.
    /// </summary>
    public static IServiceCollection AddTutorPortalModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatedOptions<TutorPortalOptions>(configuration, TutorPortalOptions.SectionName);
        services.AddSingleton<ITutorPushSettings, TutorPushSettingsAdapter>();

        services.AddDbContext<TutorPortalDbContext>((serviceProvider, options) =>
        {
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            var databaseOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            var moduleOptions = serviceProvider.GetRequiredService<IOptions<TutorPortalOptions>>().Value;
            options.ConfigureModuleDatabase(config, databaseOptions, moduleOptions.ConnectionString);
            options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        services.AddScoped<ITutorPortalAccountRepository, TutorPortalAccountRepository>();
        services.AddScoped<ITutorPushSubscriptionRepository, TutorPushSubscriptionRepository>();
        services.AddScoped<ITutorPortalUnitOfWork>(sp => sp.GetRequiredService<TutorPortalDbContext>());
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<TutorPortalDbContext>());
        services.AddScoped<IDomainEventSource>(sp => sp.GetRequiredService<TutorPortalDbContext>());
        services.AddScoped<ICrmTutorLookup, CrmTutorLookup>();
        services.AddScoped<TutorPortalAuthService>();
        services.AddScoped<TutorPortalUserResolver>();
        services.AddScoped<TutorPetAccessGuard>();
        services.AddScoped<ITutorPetHealthReadPort, TutorPetHealthReadPort>();
        services.AddScoped<ITutorSchedulingPort, TutorSchedulingPort>();

        var tutorPortalOptions = configuration.GetSection(TutorPortalOptions.SectionName).Get<TutorPortalOptions>();
        if (!string.IsNullOrWhiteSpace(tutorPortalOptions?.VapidPublicKey)
            && !string.IsNullOrWhiteSpace(tutorPortalOptions.VapidPrivateKey))
        {
            services.AddScoped<ITutorPushSender, WebPushTutorPushSender>();
        }
        else
        {
            services.AddSingleton<ITutorPushSender, FakeTutorPushSender>();
        }

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(RegisterTutorCommand).Assembly));
        services.AddValidatorsFromAssembly(typeof(RegisterTutorCommand).Assembly);

        return services;
    }
}
