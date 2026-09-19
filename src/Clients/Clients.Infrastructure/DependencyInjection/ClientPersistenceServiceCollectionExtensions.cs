using System.Linq;
using Clients.Infrastructure.Crm;
using Clients.Infrastructure.Sales;
using Clients.Infrastructure.Persistence;
using Clients.Infrastructure.Persistence.Repositories;
using Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Clients.Infrastructure.Sync;
using Microsoft.Extensions.DependencyInjection;

namespace Clients.Infrastructure.DependencyInjection;

/// <summary>
/// Registers client-local SQLite, CRM repositories, and offline-first stores.
/// </summary>
public static class ClientPersistenceServiceCollectionExtensions
{
    /// <summary>
    /// Adds <see cref="OfflineDbContext"/>, domain repositories, and offline CRM stores.
    /// </summary>
    /// <param name="services">Host service collection.</param>
    /// <param name="configureSqlite">Configures the SQLite connection (path differs per host).</param>
    public static IServiceCollection AddClientPersistence(
        this IServiceCollection services,
        string sqliteConnectionString)
    {
        return AddClientPersistence(services, options => options.UseSqlite(sqliteConnectionString));
    }

    /// <summary>
    /// Adds offline CRM persistence with custom EF options (hosts register <see cref="ISqliteFilePersistence"/> first when needed).
    /// </summary>
    public static IServiceCollection AddClientPersistence(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureSqlite)
    {
        if (!services.Any(d => d.ServiceType == typeof(ISqliteFilePersistence)))
        {
            services.AddSingleton<ISqliteFilePersistence, NoOpSqliteFilePersistence>();
        }

        services.AddDbContext<OfflineDbContext>((_, options) =>
        {
            configureSqlite(options);
            options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        services.AddScoped<ITutorRepository, OfflineTutorRepository>();
        services.AddScoped<IPetRepository, OfflinePetRepository>();
        services.AddScoped(typeof(IOfflineRepository<>), typeof(OfflineRepository<>));

        services.AddScoped<HttpTutorStore>();
        services.AddScoped<HttpPetStore>();
        services.AddScoped<ITutorStore, OfflineTutorStore>();
        services.AddScoped<IPetStore, OfflinePetStore>();
        services.AddScoped<IAppointmentStore, OfflineAppointmentStore>();
        services.AddScoped<IMedicalRecordStore, OfflineMedicalRecordStore>();
        services.AddScoped<IClinicalStore, OfflineClinicalStore>();
        services.AddScoped<IVaccineStore, OfflineVaccineStore>();
        services.AddScoped<IClinicalQuoteStore, OfflineClinicalQuoteStore>();
        services.AddScoped<IHospitalizationStore, OfflineHospitalizationStore>();
        services.AddScoped<IInventoryStore, OfflineInventoryStore>();
        services.AddScoped<ISalesStore, OfflineSalesStore>();
        services.AddSingleton<SyncWakeSignal>();
        services.AddScoped<OfflineSyncPullApplier>();

        return services;
    }

    /// <summary>
    /// Applies pending EF migrations on the local database (call after restore on WASM).
    /// </summary>
    public static async Task MigrateOfflineDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<OfflineDbContext>();
        await db.Database.MigrateAsync(cancellationToken);
    }

    /// <summary>
    /// Restores a WASM database snapshot before migrations run.
    /// </summary>
    public static async Task RestoreOfflineDatabaseIfExistsAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        var persistence = services.GetRequiredService<ISqliteFilePersistence>();
        await persistence.RestoreIfExistsAsync(cancellationToken);
    }
}
