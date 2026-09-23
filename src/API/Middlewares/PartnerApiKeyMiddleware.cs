using Core.Domain;
using Platform.Application.Abstractions;
using Platform.Domain.Repositories;
using Platform.Domain.Security;
using PlatformErrorCodes = Platform.Domain.ErrorCodes;

namespace API.Middlewares;

/// <summary>Authenticates partner routes via X-Api-Key with live DB lookup (9.7).</summary>
public sealed class PartnerApiKeyMiddleware
{
    /// <summary>Header carrying the partner API key secret.</summary>
    public const string ApiKeyHeaderName = "X-Api-Key";

    private readonly RequestDelegate _next;

    /// <summary>Creates the middleware.</summary>
    public PartnerApiKeyMiddleware(RequestDelegate next) => _next = next;

    /// <summary>Validates API keys for partner routes.</summary>
    public async Task InvokeAsync(
        HttpContext context,
        IPartnerApiKeyRepository apiKeyRepository,
        IPartnerRequestContext partnerRequestContext,
        ITenantContext tenantContext)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        if (!path.StartsWith("/api/v1/partner/", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var headerValues)
            || string.IsNullOrWhiteSpace(headerValues.ToString()))
        {
            await WriteUnauthorizedAsync(context, PlatformErrorCodes.ApiKey.MissingHeader);
            return;
        }

        var secret = headerValues.ToString().Trim();
        var hash = PartnerApiKeyHasher.HashSecret(secret);
        var key = await apiKeyRepository.GetBySecretHashAsync(hash, context.RequestAborted);
        if (key is null || !key.IsActive)
        {
            await WriteUnauthorizedAsync(context, PlatformErrorCodes.ApiKey.Invalid);
            return;
        }

        partnerRequestContext.AuthenticatedTenantId = key.TenantId;
        tenantContext.TenantId = key.TenantId;
        tenantContext.SchemaName = TenantSchema.FromId(key.TenantId);

        await _next(context);
    }

    private static Task WriteUnauthorizedAsync(HttpContext context, Core.Domain.Error error)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return context.Response.WriteAsJsonAsync(new { error = error.Message, code = error.Code });
    }
}
