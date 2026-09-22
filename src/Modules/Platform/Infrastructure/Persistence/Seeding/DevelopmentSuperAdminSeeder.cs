using Core.Application.Authorization;
using Core.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Platform.Infrastructure.Persistence.Seeding;

/// <summary>
/// Idempotently creates the development Super Admin operator (roadmap 9.2).
/// </summary>
public interface IDevelopmentSuperAdminSeeder
{
    /// <summary>Seeds the Super Admin user when the database is ready.</summary>
    Task SeedAsync(CancellationToken cancellationToken = default);
}

/// <inheritdoc />
public sealed class DevelopmentSuperAdminSeeder : IDevelopmentSuperAdminSeeder
{
    /// <summary>Default Super Admin e-mail.</summary>
    public const string SuperAdminEmail = "superadmin@vetnexus.app";

    /// <summary>Default Super Admin password.</summary>
    public const string SuperAdminPassword = "Password123!";

    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<DevelopmentSuperAdminSeeder> _logger;

    /// <summary>Creates the seeder.</summary>
    public DevelopmentSuperAdminSeeder(UserManager<AppUser> userManager, ILogger<DevelopmentSuperAdminSeeder> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var existing = await _userManager.FindByEmailAsync(SuperAdminEmail);
        if (existing is not null)
        {
            return;
        }

        var user = new AppUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = SuperAdminEmail,
            Email = SuperAdminEmail,
            TenantId = Guid.Empty,
            AccessProfileId = Guid.Empty,
            EmailConfirmed = true
        };

        var createResult = await _userManager.CreateAsync(user, SuperAdminPassword);
        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            _logger.LogWarning("Super Admin seed skipped: {Errors}", errors);
            return;
        }

        await _userManager.AddToRoleAsync(user, ApplicationRoles.SuperAdmin);
        _logger.LogInformation("Seeded Super Admin user {Email}.", SuperAdminEmail);
    }
}
