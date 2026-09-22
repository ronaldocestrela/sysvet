using ClinicSite.Domain.Repositories;
using ClinicSite.Domain.ValueObjects;
using Core.Domain;
using Microsoft.AspNetCore.Http;

namespace API.Filters;

/// <summary>
/// Resolves tenant schema from a public clinic site slug before anonymous handlers run.
/// </summary>
public sealed class ClinicSitePublicTenantFilter : IEndpointFilter
{
    /// <inheritdoc />
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var slug = context.HttpContext.Request.RouteValues["slug"] as string;
        if (string.IsNullOrWhiteSpace(slug))
        {
            return Results.BadRequest(new { error = "Slug is required." });
        }

        var normalized = PublicSiteSlug.Normalize(slug);
        var slugRepository = context.HttpContext.RequestServices.GetRequiredService<IClinicSiteSlugIndexRepository>();
        var tenantContext = context.HttpContext.RequestServices.GetRequiredService<ITenantContext>();
        var index = await slugRepository.GetBySlugAsync(normalized, context.HttpContext.RequestAborted);
        if (index is null || !index.IsPublished)
        {
            return Results.NotFound();
        }

        tenantContext.TenantId = index.TenantId;
        tenantContext.SchemaName = $"tenant_{index.TenantId:N}".ToLowerInvariant();
        return await next(context);
    }
}
