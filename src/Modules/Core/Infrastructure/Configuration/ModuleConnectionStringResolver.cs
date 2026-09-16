using Microsoft.Extensions.Configuration;

namespace Core.Infrastructure.Configuration;

/// <summary>
/// Resolves the effective connection string for a module, honoring optional per-module overrides.
/// </summary>
public static class ModuleConnectionStringResolver
{
    /// <summary>
    /// Returns the module-specific connection string when set; otherwise the shared named connection from configuration.
    /// </summary>
    /// <param name="configuration">Application configuration root.</param>
    /// <param name="databaseOptions">Shared database options (connection string name).</param>
    /// <param name="moduleConnectionStringOverride">Optional override from module options.</param>
    /// <returns>A non-empty connection string.</returns>
    /// <exception cref="InvalidOperationException">When no connection string can be resolved.</exception>
    public static string Resolve(
        IConfiguration configuration,
        DatabaseOptions databaseOptions,
        string? moduleConnectionStringOverride)
    {
        if (!string.IsNullOrWhiteSpace(moduleConnectionStringOverride))
        {
            return moduleConnectionStringOverride;
        }

        var connectionString = configuration.GetConnectionString(databaseOptions.ConnectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Connection string '{databaseOptions.ConnectionStringName}' is not configured.");
        }

        return connectionString;
    }
}
