namespace Core.Infrastructure.Persistence.Seeding;

/// <summary>
/// Seeds ASP.NET Core Identity reference data required before RBAC policies can be enforced.
/// </summary>
public interface IIdentityDataSeeder
{
    /// <summary>
    /// Ensures application roles exist; safe to call multiple times.
    /// </summary>
    Task SeedAsync(CancellationToken cancellationToken = default);
}
