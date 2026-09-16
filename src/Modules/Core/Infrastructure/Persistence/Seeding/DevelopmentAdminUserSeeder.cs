using Core.Application.Authorization;
using Core.Application.Common.Interfaces;
using Core.Domain;
using Core.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Core.Infrastructure.Persistence.Seeding;

/// <summary>
/// Idempotently creates the default development admin account documented in configuracao.md.
/// </summary>
public sealed class DevelopmentAdminUserSeeder : IDevelopmentAdminUserSeeder
{
    /// <summary>Fixed tenant id for local development CRM data.</summary>
    public static readonly Guid DevTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    /// <summary>Default development admin e-mail.</summary>
    public const string DevAdminEmail = "admin@sysvet.com";

    /// <summary>Default development admin password.</summary>
    public const string DevAdminPassword = "Password123!";

    private readonly UserManager<AppUser> _userManager;
    private readonly CoreDbContext _dbContext;
    private readonly IAccessProfileSeeder _accessProfileSeeder;
    private readonly IAccessProfileRepository _accessProfileRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<DevelopmentAdminUserSeeder> _logger;

    /// <summary>Creates the seeder with Identity and CRM dependencies.</summary>
    public DevelopmentAdminUserSeeder(
        UserManager<AppUser> userManager,
        CoreDbContext dbContext,
        IAccessProfileSeeder accessProfileSeeder,
        IAccessProfileRepository accessProfileRepository,
        ITenantContext tenantContext,
        ILogger<DevelopmentAdminUserSeeder> logger)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _accessProfileSeeder = accessProfileSeeder;
        _accessProfileRepository = accessProfileRepository;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (!await _dbContext.Database.CanConnectAsync(cancellationToken))
        {
            _logger.LogDebug("Skipping development admin seed: database is not reachable.");
            return;
        }

        var pendingMigrations = await _dbContext.Database.GetPendingMigrationsAsync(cancellationToken);
        if (pendingMigrations.Any())
        {
            _logger.LogDebug("Skipping development admin seed: {Count} pending migration(s).", pendingMigrations.Count());
            return;
        }

        _tenantContext.TenantId = DevTenantId;
        await _accessProfileSeeder.EnsureTenantProfilesAsync(DevTenantId, cancellationToken);

        var existing = await _userManager.FindByEmailAsync(DevAdminEmail);
        if (existing is not null)
        {
            return;
        }

        var adminProfile = await _accessProfileRepository.GetSystemProfileByBaseRoleAsync(ApplicationRoles.Admin, cancellationToken);
        if (adminProfile is null)
        {
            _logger.LogWarning("Skipping development admin seed: Admin access profile was not found.");
            return;
        }

        var user = new AppUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = DevAdminEmail,
            Email = DevAdminEmail,
            TenantId = DevTenantId,
            AccessProfileId = adminProfile.Id,
            EmailConfirmed = true
        };

        var createResult = await _userManager.CreateAsync(user, DevAdminPassword);
        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to seed development admin user: {errors}");
        }

        await _userManager.AddToRoleAsync(user, ApplicationRoles.Admin);
        _logger.LogInformation("Seeded development admin user {Email}.", DevAdminEmail);
    }
}
