using API.Filters;
using Commerce.Application.Commands;
using Commerce.Application.Dtos;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace API.Extensions;

/// <summary>Commerce staff and public storefront endpoints (Fase 8.8).</summary>
public static class CommerceEndpointExtensions
{
    /// <summary>Maps staff commerce routes under <c>/api/v1/commerce</c>.</summary>
    public static IEndpointRouteBuilder MapCommerceEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/v1/commerce")
            .RequireAuthorization()
            .WithTags("Commerce");

        group.MapGet("/offers", async (IMediator mediator) =>
            (await mediator.Send(new ListProductOffersQuery())).ToHttpResult());

        group.MapPut("/offers", async ([FromBody] UpsertOfferBody body, IMediator mediator) =>
            (await mediator.Send(new UpsertProductOfferCommand(
                body.ProductId,
                body.SalePrice,
                body.IsPublished,
                body.StoreEnabled,
                body.MercadoLivreEnabled))).ToHttpResult());

        group.MapGet("/orders", async (IMediator mediator) =>
            (await mediator.Send(new ListOnlineOrdersQuery())).ToHttpResult());

        group.MapPost("/orders/{orderId:guid}/ready", async (Guid orderId, IMediator mediator) =>
            (await mediator.Send(new MarkOnlineOrderReadyCommand(orderId))).ToHttpResult());

        group.MapPost("/orders/{orderId:guid}/complete", async (Guid orderId, IMediator mediator) =>
            (await mediator.Send(new CompleteOnlineOrderCommand(orderId))).ToHttpResult());

        group.MapPost("/orders/{orderId:guid}/cancel", async (Guid orderId, IMediator mediator) =>
            (await mediator.Send(new CancelOnlineOrderCommand(orderId))).ToHttpResult());

        group.MapGet("/marketplace/mercadolivre", async (IMediator mediator) =>
            (await mediator.Send(new GetMercadoLivreSettingsQuery())).ToHttpResult());

        group.MapPut("/marketplace/mercadolivre", async ([FromBody] MercadoLivreSettingsBody body, IMediator mediator) =>
            (await mediator.Send(new UpdateMercadoLivreSettingsCommand(
                body.AccessToken,
                body.UserId,
                body.SiteId ?? "MLB",
                body.IsEnabled))).ToHttpResult());

        return builder;
    }

    /// <summary>Maps public storefront routes under clinic site slug.</summary>
    public static IEndpointRouteBuilder MapCommercePublicEndpoints(this IEndpointRouteBuilder builder)
    {
        var storeGroup = builder.MapGroup("/api/v1/public/clinic-sites/{slug}/store")
            .WithTags("CommercePublic")
            .AddEndpointFilter<ClinicSitePublicTenantFilter>();

        storeGroup.MapGet("/catalog", async (IMediator mediator) =>
            (await mediator.Send(new GetPublicStoreCatalogQuery())).ToHttpResult());

        storeGroup.MapGet("/products/{offerId:guid}", async (Guid offerId, IMediator mediator) =>
            (await mediator.Send(new GetPublicStoreProductQuery(offerId))).ToHttpResult());

        storeGroup.MapPost("/orders", async ([FromBody] PlaceStoreOrderBody body, IMediator mediator) =>
            (await mediator.Send(new PlacePublicStoreOrderCommand(
                body.BuyerName,
                body.BuyerPhone,
                body.BuyerEmail,
                body.Lines ?? []))).ToHttpResult());

        var webhookGroup = builder.MapGroup("/api/v1/public/commerce/marketplaces/mercadolivre")
            .WithTags("CommerceMarketplace")
            .AddEndpointFilter<MarketplaceMercadoLivreTenantFilter>();

        webhookGroup.MapPost("/notifications", async (
            [FromQuery(Name = "user_id")] long userId,
            [FromBody] MercadoLivreNotificationBody body,
            IMediator mediator) =>
            (await mediator.Send(new ProcessMercadoLivreNotificationCommand(
                userId,
                body.Topic ?? string.Empty,
                body.Resource ?? string.Empty))).ToHttpResult());

        return builder;
    }

    private sealed record UpsertOfferBody(
        Guid ProductId,
        decimal SalePrice,
        bool IsPublished,
        bool StoreEnabled,
        bool MercadoLivreEnabled);

    private sealed record PlaceStoreOrderBody(
        string BuyerName,
        string BuyerPhone,
        string? BuyerEmail,
        IReadOnlyList<PublicStoreOrderLineDto>? Lines);

    private sealed record MercadoLivreSettingsBody(
        string? AccessToken,
        long? UserId,
        string? SiteId,
        bool IsEnabled);

    private sealed record MercadoLivreNotificationBody(string? Topic, string? Resource);
}
