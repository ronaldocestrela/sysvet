namespace Platform.Application.Abstractions;

/// <summary>Issues short-lived JWT for tenant impersonation (9.6).</summary>
public interface IImpersonationAccessTokenIssuer
{
    /// <summary>Creates bearer token with impersonation claims.</summary>
    /// <param name="actorUserId">Support operator id.</param>
    /// <param name="actorEmail">Support operator e-mail.</param>
    /// <param name="targetTenantId">Tenant being accessed.</param>
    /// <param name="sessionId">Impersonation session id.</param>
    /// <param name="lifetimeMinutes">Token lifetime.</param>
    ImpersonationTokenResult Issue(
        string actorUserId,
        string actorEmail,
        Guid targetTenantId,
        Guid sessionId,
        int lifetimeMinutes);
}

/// <summary>Impersonation JWT payload returned to Super Admin.</summary>
/// <param name="AccessToken">Bearer token.</param>
/// <param name="ExpiresInSeconds">Lifetime in seconds.</param>
/// <param name="SessionId">Session id for end call.</param>
public sealed record ImpersonationTokenResult(string AccessToken, int ExpiresInSeconds, Guid SessionId);
