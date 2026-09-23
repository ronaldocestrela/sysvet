using API.Filters;
using Platform.Application.Impersonation;
using Platform.Domain.Repositories;
using PlatformErrorCodes = Platform.Domain.ErrorCodes;

namespace API.Middlewares;

/// <summary>Validates impersonation JWT sessions against catalog state (9.6).</summary>
public sealed class ImpersonationSessionMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>Creates the middleware.</summary>
    public ImpersonationSessionMiddleware(RequestDelegate next) => _next = next;

    /// <summary>Runs session validation when impersonation claim is present.</summary>
    public async Task InvokeAsync(HttpContext context, IImpersonationSessionRepository sessionRepository)
    {
        if (TenantEndpointAllowlist.IsExempt(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var sessionClaim = context.User.FindFirst(ImpersonationClaimTypes.SessionId)?.Value;
        if (string.IsNullOrWhiteSpace(sessionClaim))
        {
            await _next(context);
            return;
        }

        if (!Guid.TryParse(sessionClaim, out var sessionId))
        {
            await WriteUnauthorizedAsync(context);
            return;
        }

        var session = await sessionRepository.GetByIdAsync(sessionId, context.RequestAborted);
        if (session is null || !session.IsActive(DateTimeOffset.UtcNow))
        {
            await WriteUnauthorizedAsync(context);
            return;
        }

        await _next(context);
    }

    private static Task WriteUnauthorizedAsync(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return context.Response.WriteAsJsonAsync(new
        {
            error = PlatformErrorCodes.Impersonation.SessionExpired.Message,
            code = PlatformErrorCodes.Impersonation.SessionExpired.Code
        });
    }
}
