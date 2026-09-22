using Core.Domain;
using Core.Infrastructure.Configuration;
using Core.Infrastructure.Persistence;
using Core.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Commerce.Infrastructure.Persistence;

/// <summary>Design-time factory for Commerce EF migrations.</summary>
public sealed class CommerceDbContextFactory : IDesignTimeDbContextFactory<CommerceDbContext>
{
    /// <inheritdoc />
    public CommerceDbContext CreateDbContext(string[] args)
    {
        var apiPath = ResolveApiProjectPath();
        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiPath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var databaseOptions = configuration.GetSection("Database").Get<DatabaseOptions>() ?? new DatabaseOptions();
        var tenancySettings = Options.Create(new TenancySettings { DefaultSchema = "dbo" });
        ITenantContext tenantContext = new DefaultTenantContext(tenancySettings);

        var optionsBuilder = new DbContextOptionsBuilder<CommerceDbContext>();
        optionsBuilder.ConfigureModuleDatabase(configuration, databaseOptions);
        return new CommerceDbContext(optionsBuilder.Options, tenantContext);
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
