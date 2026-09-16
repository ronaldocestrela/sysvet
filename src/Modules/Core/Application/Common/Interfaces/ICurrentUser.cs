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
    /// Evaluates a named authorization policy for the current user.
    /// </summary>
    Task<bool> IsInPolicyAsync(string policyName, CancellationToken cancellationToken = default);
}
