namespace Core.Application.Common.Interfaces;

/// <summary>
/// Abstraction over the authenticated caller for authorization behaviors without referencing ASP.NET types.
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// Whether the current HTTP context has an authenticated user.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Subject identifier from the JWT (<c>sub</c> / name identifier).
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Email claim when present.
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Tenant identifier from the custom <c>TenantId</c> claim.
    /// </summary>
    Guid TenantId { get; }

    /// <summary>
    /// Role claims attached to the access token.
    /// </summary>
    IReadOnlyList<string> Roles { get; }

    /// <summary>
    /// Evaluates a named authorization policy for the current user.
    /// </summary>
    Task<bool> IsInPolicyAsync(string policyName, CancellationToken cancellationToken = default);
}
