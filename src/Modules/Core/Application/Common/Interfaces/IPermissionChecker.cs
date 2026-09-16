namespace Core.Application.Common.Interfaces;

/// <summary>
/// Resolves fine-grained permissions for the authenticated user from their access profile.
/// </summary>
public interface IPermissionChecker
{
    /// <summary>
    /// Returns whether the current user has the given catalog permission.
    /// </summary>
    Task<bool> HasPermissionAsync(string permissionCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all permission codes granted to the current user.
    /// </summary>
    Task<IReadOnlyList<string>> GetGrantedPermissionsAsync(CancellationToken cancellationToken = default);
}
