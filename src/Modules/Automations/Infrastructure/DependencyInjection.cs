using Automations.Application.Abstractions;
using Automations.Application.Jobs;
using Automations.Application.Jobs.Commands;
using Automations.Domain.Repositories;
using Automations.Infrastructure.Channels;
using Automations.Infrastructure.Configuration;
using Automations.Infrastructure.Notifications;
using Automations.Infrastructure.Persistence;
using Automations.Infrastructure.Persistence.Repositories;
using Automations.Infrastructure.Persistence.Seeding;
using Automations.Infrastructure.Workers;
using Core.Application.Notifications;
using Core.Domain;
using Core.Infrastructure.Configuration;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Automations.Infrastructure;

/// <summary>
/// Registers Automations module persistence, worker, and tutor notification channel.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Automations DbContext, repositories, MediatR handlers, and outbox worker.
    /// </summary>
    public static IServiceCollection AddAutomationsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatedOptions<AutomationsOptions>(configuration, AutomationsOptions.SectionName);

        services.AddDbContext<AutomationsDbContext>((serviceProvider, options) =>
        {
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            var databaseOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            var moduleOptions = serviceProvider.GetRequiredService<IOptions<AutomationsOptions>>().Value;
            options.ConfigureModuleDatabase(config, databaseOptions, moduleOptions.ConnectionString);
            options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        services.AddScoped<IMessageJobRepository, MessageJobRepository>();
        services.AddScoped<IMessageTemplateRepository, MessageTemplateRepository>();
        services.AddScoped<IAutomationsUnitOfWork>(sp => sp.GetRequiredService<AutomationsDbContext>());
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AutomationsDbContext>());
        services.AddScoped<IDomainEventSource>(sp => sp.GetRequiredService<AutomationsDbContext>());

        services.AddScoped<IOutboundMessageSender, LoggingOutboundMessageSender>();
        services.AddScoped<MessageJobProcessor>();

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(EnqueueMessageJobCommand).Assembly));

        services.AddValidatorsFromAssembly(typeof(EnqueueMessageJobCommand).Assembly);

        services.RemoveAll<ITutorNotificationChannel>();
        services.AddScoped<ITutorNotificationChannel, EnqueueingTutorNotificationChannel>();

        services.AddHostedService<OutboxProcessor>();
        services.AddHostedService<AutomationsTemplateSeedHostedService>();

        return services;
    }
}
