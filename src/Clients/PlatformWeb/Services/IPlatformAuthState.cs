using SharedUI.Services;

namespace PlatformWeb.Services;

/// <summary>Authentication state for PlatformWeb including Super Admin role gate.</summary>
public interface IPlatformAuthState : IAuthState
{
    /// <summary>Roles from the last <c>/auth/me</c> response.</summary>
    IReadOnlyList<string> Roles { get; }

    /// <summary>Whether the current user has the Super Admin role.</summary>
    bool IsSuperAdmin { get; }

    /// <summary>Persists roles after profile load.</summary>
    Task SetRolesAsync(IReadOnlyList<string> roles);
}
