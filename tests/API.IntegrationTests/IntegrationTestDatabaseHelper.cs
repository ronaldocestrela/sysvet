using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace API.IntegrationTests;

/// <summary>
/// Applies EF migrations for module databases used by integration and sync tests.
/// </summary>
internal static class IntegrationTestDatabaseHelper
{
    /// <summary>Matches <c>TenancySettings:SingleTenantId</c> in integration test configuration.</summary>
    internal static readonly Guid SingleTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    /// <summary>
    /// Migrates module schemas used by sync pull/push (Veterinary, Inventory, Sales).
    /// </summary>
    public static async Task ResetModuleDatabasesAsync(IServiceScope scope)
    {
        foreach (var ctx in new DbContext[]
                 {
                     scope.ServiceProvider.GetRequiredService<global::Veterinary.Infrastructure.Persistence.VeterinaryDbContext>(),
                     scope.ServiceProvider.GetRequiredService<global::Inventory.Infrastructure.Persistence.InventoryDbContext>(),
                     scope.ServiceProvider.GetRequiredService<global::Sales.Infrastructure.Persistence.SalesDbContext>(),
                     scope.ServiceProvider.GetRequiredService<global::Petshop.Infrastructure.Persistence.PetshopDbContext>(),
                     scope.ServiceProvider.GetRequiredService<global::Finance.Infrastructure.Persistence.FinanceDbContext>(),
                     scope.ServiceProvider.GetRequiredService<global::Fiscal.Infrastructure.Persistence.FiscalDbContext>(),
                     scope.ServiceProvider.GetRequiredService<global::Automations.Infrastructure.Persistence.AutomationsDbContext>()
                 })
        {
            await ctx.Database.EnsureDeletedAsync();
        }
    }

    public static async Task MigrateModuleDatabasesAsync(IServiceScope scope)
    {
        var vetContext = scope.ServiceProvider.GetRequiredService<global::Veterinary.Infrastructure.Persistence.VeterinaryDbContext>();
        await vetContext.Database.MigrateAsync();
        await EnsureMedicalRecordFollowUpColumnAsync(vetContext);

        var inventoryContext = scope.ServiceProvider.GetRequiredService<global::Inventory.Infrastructure.Persistence.InventoryDbContext>();
        await inventoryContext.Database.MigrateAsync();

        var salesContext = scope.ServiceProvider.GetRequiredService<global::Sales.Infrastructure.Persistence.SalesDbContext>();
        await salesContext.Database.MigrateAsync();

        var petshopContext = scope.ServiceProvider.GetRequiredService<global::Petshop.Infrastructure.Persistence.PetshopDbContext>();
        await petshopContext.Database.MigrateAsync();

        var financeContext = scope.ServiceProvider.GetRequiredService<global::Finance.Infrastructure.Persistence.FinanceDbContext>();
        await financeContext.Database.MigrateAsync();

        var fiscalContext = scope.ServiceProvider.GetRequiredService<global::Fiscal.Infrastructure.Persistence.FiscalDbContext>();
        await fiscalContext.Database.MigrateAsync();

        var automationsContext = scope.ServiceProvider.GetRequiredService<global::Automations.Infrastructure.Persistence.AutomationsDbContext>();
        await automationsContext.Database.MigrateAsync();
        await EnsureAutomations821SchemaAsync(automationsContext);
    }

    private static async Task EnsureAutomations821SchemaAsync(global::Automations.Infrastructure.Persistence.AutomationsDbContext context)
    {
        await context.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS AutomationsSettings (
                Id TEXT NOT NULL PRIMARY KEY,
                Key TEXT NOT NULL,
                TimeZoneId TEXT NOT NULL,
                BusinessStart TEXT NOT NULL,
                BusinessEnd TEXT NOT NULL,
                BusinessDaysJson TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL,
                RowVersion BLOB NOT NULL
            );
            CREATE UNIQUE INDEX IF NOT EXISTS IX_AutomationsSettings_Key ON AutomationsSettings (Key);
            CREATE TABLE IF NOT EXISTS TutorMessagingPreferences (
                Id TEXT NOT NULL PRIMARY KEY,
                TutorId TEXT NOT NULL,
                WhatsAppEnabled INTEGER NOT NULL,
                EmailEnabled INTEGER NOT NULL,
                UpdatedAt TEXT NOT NULL,
                RowVersion BLOB NOT NULL
            );
            CREATE UNIQUE INDEX IF NOT EXISTS IX_TutorMessagingPreferences_TutorId ON TutorMessagingPreferences (TutorId);
            """);
    }

    private static async Task EnsureMedicalRecordFollowUpColumnAsync(global::Veterinary.Infrastructure.Persistence.VeterinaryDbContext context)
    {
        try
        {
            await context.Database.ExecuteSqlRawAsync("ALTER TABLE MedicalRecords ADD COLUMN FollowUpOn TEXT NULL;");
        }
        catch
        {
            // Column already exists.
        }
    }
}
