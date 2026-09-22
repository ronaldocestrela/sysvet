using Core.Application.Sync;
using Core.Domain;
using Core.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Petshop.Application.GroomingAppointments;
using Petshop.Application.GroomingAppointments.Commands;
using Petshop.Domain.Repositories;
using Petshop.Infrastructure.Configuration;
using Petshop.Infrastructure.Persistence;
using Petshop.Infrastructure.Persistence.Repositories;
using Petshop.Infrastructure.Sync;

namespace Petshop.Infrastructure;

/// <summary>
/// Registers Petshop module persistence, repositories, and MediatR handlers.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Petshop DbContext, repositories, validators, and command/query handlers.
    /// </summary>
    public static IServiceCollection AddPetshopModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatedOptions<PetshopOptions>(configuration, PetshopOptions.SectionName);

        services.AddDbContext<PetshopDbContext>((serviceProvider, options) =>
        {
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            var databaseOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            var moduleOptions = serviceProvider.GetRequiredService<IOptions<PetshopOptions>>().Value;
            options.ConfigureModuleDatabase(config, databaseOptions, moduleOptions.ConnectionString);
            options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        services.AddScoped<IGroomingAppointmentScheduler, GroomingAppointmentScheduler>();
        services.AddScoped<IGroomingAppointmentRepository, GroomingAppointmentRepository>();
        services.AddScoped<IGroomingSlotRepository, GroomingSlotRepository>();
        services.AddScoped<IGroomingRecordRepository, GroomingRecordRepository>();
        services.AddScoped<IGroomingServiceRepository, GroomingServiceRepository>();
        services.AddScoped<IPetshopUnitOfWork>(provider => provider.GetRequiredService<PetshopDbContext>());
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<PetshopDbContext>());
        services.AddScoped<IDomainEventSource>(provider => provider.GetRequiredService<PetshopDbContext>());

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(ScheduleGroomingAppointmentCommand).Assembly));

        FluentValidation.ServiceCollectionExtensions.AddValidatorsFromAssembly(services, typeof(ScheduleGroomingAppointmentCommand).Assembly);

        services.AddScoped<ISyncPushHandler, PetshopSyncPushHandler>();
        services.AddScoped<ISyncChangeFeedContributor, PetshopSyncChangeFeedContributor>();

        return services;
    }
}
