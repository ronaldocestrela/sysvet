using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace API.IntegrationTests;

/// <summary>
/// Applies EF migrations for module databases used by integration and sync tests.
/// </summary>
internal static class IntegrationTestDatabaseHelper
{
    /// <summary>
    /// Migrates Veterinary and Inventory schemas (sync pull reads both feeds).
    /// </summary>
    public static async Task MigrateModuleDatabasesAsync(IServiceScope scope)
    {
        var vetContext = scope.ServiceProvider.GetRequiredService<global::Veterinary.Infrastructure.Persistence.VeterinaryDbContext>();
        await vetContext.Database.MigrateAsync();

        var inventoryContext = scope.ServiceProvider.GetRequiredService<global::Inventory.Infrastructure.Persistence.InventoryDbContext>();
        await inventoryContext.Database.MigrateAsync();
    }
}
