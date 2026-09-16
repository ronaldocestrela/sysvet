namespace Core.Infrastructure.Persistence.Seeding;

/// <summary>
/// Seeds a default clinic admin user for local development only.
/// </summary>
public interface IDevelopmentAdminUserSeeder
{
    /// <summary>
    /// Ensures <c>admin@sysvet.com</c> exists with the Admin role and system access profile.
    /// </summary>
    Task SeedAsync(CancellationToken cancellationToken = default);
}
