using Commerce.Domain.Repositories;
using Core.Domain;
using Microsoft.AspNetCore.Http;

namespace API.Filters;

/// <summary>
/// Resolves tenant from Mercado Livre seller user id before webhook handlers run.
/// </summary>
public sealed class MarketplaceMercadoLivreTenantFilter : IEndpointFilter
{
    /// <inheritdoc />
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (!context.HttpContext.Request.Query.TryGetValue("user_id", out var userIdValues) ||
            !long.TryParse(userIdValues.FirstOrDefault(), out var userId))
        {
            return Results.BadRequest(new { error = "user_id query parameter is required." });
        }

        var repository = context.HttpContext.RequestServices.GetRequiredService<IMarketplaceSellerIndexRepository>();
        var tenantContext = context.HttpContext.RequestServices.GetRequiredService<ITenantContext>();
        var index = await repository.GetByMercadoLivreUserIdAsync(userId, context.HttpContext.RequestAborted);
        if (index is null)
        {
            return Results.NotFound();
        }

        tenantContext.TenantId = index.TenantId;
        tenantContext.SchemaName = TenantSchema.FromId(index.TenantId);
        return await next(context);
    }
}
