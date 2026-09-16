using Core.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Core.Infrastructure.Identity;

/// <summary>
/// Resolves authorization policies from the current HTTP context for CQRS pipeline behaviors.
/// </summary>
public class HttpCurrentUser(IHttpContextAccessor httpContextAccessor, IAuthorizationService authorizationService) : ICurrentUser
{
    public bool IsAuthenticated =>
        httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;

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
