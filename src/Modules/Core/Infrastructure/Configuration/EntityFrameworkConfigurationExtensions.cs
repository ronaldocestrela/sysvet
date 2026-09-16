using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Core.Infrastructure.Configuration;

/// <summary>
/// Centralizes EF Core provider selection so modules do not hardcode SQLite paths.
/// </summary>
public static class EntityFrameworkConfigurationExtensions
{
    /// <summary>
    /// Configures the DbContext to use SQLite or SQL Server based on <paramref name="databaseOptions"/>.
    /// </summary>
    /// <param name="optionsBuilder">The options builder passed to <c>AddDbContext</c>.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <param name="databaseOptions">Provider and default connection string name.</param>
    /// <param name="moduleConnectionStringOverride">Optional module-specific connection string.</param>
    public static void ConfigureModuleDatabase(
        this DbContextOptionsBuilder optionsBuilder,
        IConfiguration configuration,
        DatabaseOptions databaseOptions,
        string? moduleConnectionStringOverride = null)
    {
        var connectionString = ModuleConnectionStringResolver.Resolve(
            configuration,
            databaseOptions,
            moduleConnectionStringOverride);

        if (string.Equals(databaseOptions.Provider, "Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            optionsBuilder.UseSqlite(connectionString);
            return;
        }

        if (string.Equals(databaseOptions.Provider, "SqlServer", StringComparison.OrdinalIgnoreCase))
        {
            optionsBuilder.UseSqlServer(connectionString);
            return;
        }

        throw new InvalidOperationException(
            $"Unsupported database provider '{databaseOptions.Provider}'. Use Sqlite or SqlServer.");
    }
}
