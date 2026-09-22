using Core.Application.Auth.Dtos;
using Core.Application.Common;
using Core.Application.Users.Dtos;
using Core.Domain;

namespace Core.Application.Common.Interfaces;

/// <summary>
/// Abstracts ASP.NET Core Identity for use cases without referencing infrastructure types.
/// </summary>
public interface IIdentityService
{
    /// <summary>
    /// Validates email and password, applying lockout and disabled rules on failure.
    /// </summary>
    Task<Result<AuthenticatedUserDto>> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a user with the given role, profile, and tenant.
    /// </summary>
    Task<Result<string>> CreateUserAsync(
        string email,
        string password,
        string role,
        Guid tenantId,
        Guid accessProfileId,
        string? displayName = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a tutor portal Identity user without an access profile.
    /// </summary>
    Task<Result<string>> CreateTutorUserAsync(
        string email,
        string password,
        Guid tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a staff user by id within the tenant.
    /// </summary>
    Task<Result<StaffUserDto>> GetByIdAsync(string userId, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists staff users for a tenant with paging and optional filters.
    /// </summary>
    Task<Result<PagedResult<StaffUserDto>>> ListByTenantAsync(
        Guid tenantId,
        int page,
        int pageSize,
        string? nameOrEmailFilter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates display name and access profile, synchronizing Identity role from profile base role.
    /// </summary>
    Task<Result> UpdateProfileAndNameAsync(
        string userId,
        Guid tenantId,
        Guid accessProfileId,
        string? displayName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets whether the user account is disabled.
    /// </summary>
    Task<Result> SetDisabledAsync(string userId, Guid tenantId, bool disabled, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets the user's password.
    /// </summary>
    Task<Result> ResetPasswordAsync(string userId, Guid tenantId, string newPassword, CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces the user's single Identity role.
    /// </summary>
    Task<Result> ReplaceRoleAsync(string userId, string role, CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts active admin users in the tenant.
    /// </summary>
    Task<int> CountAdminsInTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts users assigned to an access profile.
    /// </summary>
    Task<int> CountUsersWithProfileAsync(Guid accessProfileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ensures the user has an access profile id, assigning the system profile for their role when missing.
    /// </summary>
    Task<Result<Guid>> EnsureAccessProfileAsync(string userId, Guid tenantId, string role, CancellationToken cancellationToken = default);
}
