namespace API.Middlewares;

/// <summary>
/// Adds baseline HTTP security headers for browser clients.
/// </summary>
public sealed class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>Creates the middleware.</summary>
    public SecurityHeadersMiddleware(RequestDelegate next) => _next = next;

    /// <summary>Applies security headers and continues the pipeline.</summary>
    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;
        headers["X-Content-Type-Options"] = "nosniff";
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        headers["X-Frame-Options"] = "DENY";
        headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";

        await _next(context);
    }
}
