using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Core.Application.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Platform.Application.Abstractions;
using Platform.Application.Impersonation;
using Core.Infrastructure.Identity;

namespace Platform.Infrastructure.Identity;

/// <summary>Issues short-lived impersonation JWTs (9.6).</summary>
public sealed class ImpersonationAccessTokenIssuer : IImpersonationAccessTokenIssuer
{
    private readonly JwtSettings _jwtSettings;

    /// <summary>Creates the issuer.</summary>
    public ImpersonationAccessTokenIssuer(IOptions<JwtSettings> jwtSettingsOptions) =>
        _jwtSettings = jwtSettingsOptions.Value;

    /// <inheritdoc />
    public ImpersonationTokenResult Issue(
        string actorUserId,
        string actorEmail,
        Guid targetTenantId,
        Guid sessionId,
        int lifetimeMinutes)
    {
        var lifetime = Math.Clamp(lifetimeMinutes, 1, 120);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, actorUserId),
            new(ClaimTypes.NameIdentifier, actorUserId),
            new(JwtRegisteredClaimNames.Email, actorEmail),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("TenantId", targetTenantId.ToString()),
            new(ClaimTypes.Role, ApplicationRoles.Admin),
            new(ImpersonationClaimTypes.SessionId, sessionId.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(lifetime);
        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        var serialized = new JwtSecurityTokenHandler().WriteToken(token);
        return new ImpersonationTokenResult(serialized, lifetime * 60, sessionId);
    }
}
