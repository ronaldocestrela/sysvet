using Core.Application.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Core.Infrastructure.Persistence.Seeding;

/// <summary>
/// Idempotently creates Identity roles referenced by <see cref="ApplicationRoles"/>.
/// </summary>
public sealed class IdentityDataSeeder : IIdentityDataSeeder
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly CoreDbContext _dbContext;
    private readonly ILogger<IdentityDataSeeder> _logger;

    public IdentityDataSeeder(
        RoleManager<IdentityRole> roleManager,
        CoreDbContext dbContext,
        ILogger<IdentityDataSeeder> logger)
    {
        _roleManager = roleManager;
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (!await _dbContext.Database.CanConnectAsync(cancellationToken))
        {
            _logger.LogDebug("Skipping identity seed: database is not reachable.");
            return;
        }

        var pendingMigrations = await _dbContext.Database.GetPendingMigrationsAsync(cancellationToken);
        if (pendingMigrations.Any())
        {
            _logger.LogDebug("Skipping identity seed: {Count} pending migration(s).", pendingMigrations.Count());
            return;
        }

        foreach (var roleName in ApplicationRoles.All)
        {
            if (await _roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to seed role '{roleName}': {errors}");
            }

            _logger.LogInformation("Seeded Identity role {RoleName}.", roleName);
        }
    }
}
