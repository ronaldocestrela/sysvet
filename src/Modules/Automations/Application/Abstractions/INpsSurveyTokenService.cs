using Core.Domain;

namespace Automations.Application.Abstractions;

/// <summary>
/// Signs and validates public NPS survey tokens without JWT.
/// </summary>
public interface INpsSurveyTokenService
{
    /// <summary>
    /// Builds a URL-safe token embedding tenant and invite identifiers.
    /// </summary>
    string CreateToken(Guid tenantId, Guid inviteId, DateTimeOffset expiresAt);

    /// <summary>
    /// Validates signature and expiry, returning parsed identifiers.
    /// </summary>
    Result<(Guid TenantId, Guid InviteId)> Validate(string token, DateTimeOffset now);

    /// <summary>
    /// Computes a stable hash stored alongside the invite row.
    /// </summary>
    string ComputeTokenHash(string token);
}
