using Core.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Veterinary.Application.Appointments.Commands;
using Core.Domain;
using Veterinary.Domain.Repositories;
using Veterinary.Infrastructure.Configuration;
using Veterinary.Infrastructure.Persistence;
using Core.Application.Sync;
using Veterinary.Infrastructure.Persistence.Repositories;
using Veterinary.Infrastructure.Sync;

namespace Veterinary.Infrastructure;

/// <summary>
/// Registers Veterinary module persistence, repositories, and MediatR handlers.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Veterinary DbContext, repositories, validators, and command/query handlers.
    /// </summary>
    public static IServiceCollection AddVeterinaryModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatedOptions<VeterinaryOptions>(configuration, VeterinaryOptions.SectionName);

        services.AddDbContext<VeterinaryDbContext>((serviceProvider, options) =>
        {
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            var databaseOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            var moduleOptions = serviceProvider.GetRequiredService<IOptions<VeterinaryOptions>>().Value;
            options.ConfigureModuleDatabase(config, databaseOptions, moduleOptions.ConnectionString);
        });

        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IScheduleSlotRepository, ScheduleSlotRepository>();
        services.AddScoped<IMedicalRecordRepository, MedicalRecordRepository>();
        services.AddScoped<IVaccineDoseRepository, VaccineDoseRepository>();
        services.AddScoped<IHospitalizationRepository, HospitalizationRepository>();
        services.AddScoped<IPrescriptionExecutionRepository, PrescriptionExecutionRepository>();
        services.AddScoped<IPrescriptionTemplateRepository, PrescriptionTemplateRepository>();
        services.AddScoped<IIssuedPrescriptionRepository, IssuedPrescriptionRepository>();
        services.AddScoped<IClinicalExamRepository, ClinicalExamRepository>();
        services.AddScoped<IClinicalAttachmentRepository, ClinicalAttachmentRepository>();
        services.AddScoped<IVeterinaryUnitOfWork>(provider => provider.GetRequiredService<VeterinaryDbContext>());
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<VeterinaryDbContext>());

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(ScheduleAppointmentCommand).Assembly));

        FluentValidation.ServiceCollectionExtensions.AddValidatorsFromAssembly(services, typeof(ScheduleAppointmentCommand).Assembly);

        services.AddScoped<ISyncPushHandler, VeterinarySyncPushHandler>();
        services.AddScoped<ISyncChangeFeedContributor, VeterinarySyncChangeFeedContributor>();

        return services;
    }
}
