using Core.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Core.Infrastructure.Persistence;

/// <summary>
/// Design-time factory so EF Core CLI can build <see cref="CoreDbContext"/> without the API host.
/// </summary>
public sealed class CoreDbContextFactory : IDesignTimeDbContextFactory<CoreDbContext>
{
    /// <inheritdoc />
    public CoreDbContext CreateDbContext(string[] args)
    {
        var apiPath = ResolveApiProjectPath();
        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiPath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection") ?? "Data Source=sysvet-design.db";
        var optionsBuilder = new DbContextOptionsBuilder<CoreDbContext>();
        optionsBuilder.UseSqlite(connectionString);

        var tenancySettings = Options.Create(new TenancySettings { DefaultSchema = "dbo" });
        var tenantContext = new DefaultTenantContext(tenancySettings);

        return new CoreDbContext(optionsBuilder.Options, tenantContext);
    }

    private static string ResolveApiProjectPath()
    {
        var candidates = new List<string>
        {
            Directory.GetCurrentDirectory(),
            Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "API")),
            Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "API")),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "..", "API")),
        };

        foreach (var candidate in candidates.Distinct(StringComparer.Ordinal))
        {
            if (File.Exists(Path.Combine(candidate, "appsettings.json")))
            {
                return candidate;
            }
        }

        throw new InvalidOperationException(
            "Could not locate src/API for design-time configuration. Run EF commands from the repository root.");
    }
}
