using System.Security.Claims;
using Core.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Core.Infrastructure.Identity;

/// <summary>
/// Resolves authorization policies from the current HTTP context for CQRS pipeline behaviors.
/// </summary>
public class HttpCurrentUser(IHttpContextAccessor httpContextAccessor, IAuthorizationService authorizationService) : ICurrentUser
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated =>
        Principal?.Identity?.IsAuthenticated == true;

    public string? UserId =>
        Principal?.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? Principal?.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);

    public string? Email =>
        Principal?.FindFirstValue(ClaimTypes.Email)
        ?? Principal?.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email);

    public Guid TenantId
    {
        get
        {
            var claim = Principal?.FindFirst("TenantId")?.Value
                ?? Principal?.FindFirst(c => c.Type.Equals("TenantId", StringComparison.OrdinalIgnoreCase))?.Value;
            return claim is not null && Guid.TryParse(claim, out var tenantId) ? tenantId : Guid.Empty;
        }
    }

    public Guid AccessProfileId
    {
        get
        {
            var claim = Principal?.FindFirst("AccessProfileId")?.Value;
            return claim is not null && Guid.TryParse(claim, out var profileId) ? profileId : Guid.Empty;
        }
    }

    public Guid? TutorId
    {
        get
        {
            var claim = Principal?.FindFirst("TutorId")?.Value;
            return claim is not null && Guid.TryParse(claim, out var tutorId) ? tutorId : null;
        }
    }

    public IReadOnlyList<string> Roles =>
        Principal?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList() ?? [];

    public async Task<bool> IsInPolicyAsync(string policyName, CancellationToken cancellationToken = default)
    {
        var context = httpContextAccessor.HttpContext;
        if (context?.User is null)
        {
            return false;
        }

        var result = await authorizationService.AuthorizeAsync(context.User, policyName);
        return result.Succeeded;
    }
}
