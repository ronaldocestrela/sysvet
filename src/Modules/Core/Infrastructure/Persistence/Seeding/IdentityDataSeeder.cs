using Core.Application.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Core.Infrastructure.Persistence.Seeding;

/// <summary>
/// Idempotently creates Identity roles referenced by <see cref="ApplicationRoles"/>.
/// </summary>
public sealed class IdentityDataSeeder : IIdentityDataSeeder
{
    private const int MaxCreateAttempts = 3;

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

        foreach (var roleName in ApplicationRoles.AllIncludingTutor)
        {
            await EnsureRoleExistsAsync(roleName, cancellationToken);
        }
    }

    private async Task EnsureRoleExistsAsync(string roleName, CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= MaxCreateAttempts; attempt++)
        {
            if (await _roleManager.RoleExistsAsync(roleName))
            {
                return;
            }

            try
            {
                var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
                if (result.Succeeded)
                {
                    _logger.LogInformation("Seeded Identity role {RoleName}.", roleName);
                    return;
                }

                if (await _roleManager.RoleExistsAsync(roleName) ||
                    result.Errors.Any(e => e.Code is "DuplicateRoleName" or "DuplicateRoleId"))
                {
                    return;
                }

                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to seed role '{roleName}': {errors}");
            }
            catch (Exception ex) when (ex is not InvalidOperationException && IsRetryableSeedConflict(ex))
            {
                _dbContext.ChangeTracker.Clear();

                if (await _roleManager.RoleExistsAsync(roleName))
                {
                    return;
                }

                if (attempt == MaxCreateAttempts)
                {
                    throw;
                }

                await Task.Delay(50 * attempt, cancellationToken);
            }
        }
    }

    private static bool IsRetryableSeedConflict(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is SqliteException sqlite)
            {
                return sqlite.SqliteErrorCode is 5 or 6 or 19;
            }
        }

        return false;
    }
}
