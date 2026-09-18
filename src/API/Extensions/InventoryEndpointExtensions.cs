using Inventory.Application.ProductLots.Commands;
using Inventory.Application.StockMovements.Commands;
using Inventory.Application.Products.Commands;
using Inventory.Application.Products.Queries;
using Inventory.Application.Suppliers.Commands;
using Inventory.Application.Suppliers.Queries;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace API.Extensions;

/// <summary>
/// Inventory module minimal API endpoints.
/// </summary>
public static class InventoryEndpointExtensions
{
    /// <summary>
    /// Maps inventory product and stock movement routes under <c>/api/v1/inventory</c>.
    /// </summary>
    public static IEndpointRouteBuilder MapInventoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/inventory")
            .RequireAuthorization()
            .WithTags("Inventory");

        group.MapGet("/products", async ([FromQuery] bool? activeOnly, IMediator mediator) =>
            (await mediator.Send(new ListProductsQuery(activeOnly ?? true))).ToHttpResult());

        group.MapGet("/products/{id:guid}", async (Guid id, IMediator mediator) =>
            (await mediator.Send(new GetProductByIdQuery(id))).ToHttpResult());

        group.MapGet("/products/by-sku/{sku}", async (string sku, IMediator mediator) =>
            (await mediator.Send(new GetProductBySkuQuery(sku))).ToHttpResult());

        group.MapGet("/products/by-barcode/{barcode}", async (string barcode, IMediator mediator) =>
            (await mediator.Send(new GetProductByBarcodeQuery(barcode))).ToHttpResult());

        group.MapPost("/products", async (HttpContext httpContext, [FromBody] RegisterProductCommand command, IMediator mediator) =>
        {
            var key = EndpointIdempotency.ReadKey(httpContext);
            return (await mediator.Send(command with { IdempotencyKey = key })).ToHttpResult();
        });

        group.MapPut("/products/{id:guid}", async (Guid id, HttpContext httpContext, [FromBody] UpdateProductBody body, IMediator mediator) =>
        {
            var key = EndpointIdempotency.ReadKey(httpContext);
            var command = new UpdateProductCommand(
                id,
                body.Name,
                body.Description,
                body.Sku,
                body.Barcode,
                body.UnitOfMeasure,
                body.ReorderLevel,
                body.Category,
                body.Ncm,
                body.Cest,
                body.MerchandiseOrigin,
                body.SupplierId,
                body.RequiresLot,
                key);
            return (await mediator.Send(command)).ToHttpResult();
        });

        group.MapPost("/products/{id:guid}/active", async (Guid id, HttpContext httpContext, [FromBody] SetActiveBody body, IMediator mediator) =>
        {
            var key = EndpointIdempotency.ReadKey(httpContext);
            return (await mediator.Send(new SetProductActiveCommand(id, body.IsActive, key))).ToHttpResult();
        });

        group.MapPost("/products/{productId:guid}/lots", async (Guid productId, HttpContext httpContext, [FromBody] RegisterProductLotBody body, IMediator mediator) =>
        {
            var key = EndpointIdempotency.ReadKey(httpContext);
            var command = new RegisterProductLotCommand(
                productId,
                body.LotNumber,
                body.ExpirationDate,
                body.UnitCost,
                body.InitialQuantity,
                body.LotId,
                key);
            return (await mediator.Send(command)).ToHttpResult();
        });

        group.MapPut("/lots/{lotId:guid}", async (Guid lotId, HttpContext httpContext, [FromBody] UpdateProductLotBody body, IMediator mediator) =>
        {
            var key = EndpointIdempotency.ReadKey(httpContext);
            return (await mediator.Send(new UpdateProductLotCommand(lotId, body.ExpirationDate, body.UnitCost, key))).ToHttpResult();
        });

        group.MapPost("/lots/{lotId:guid}/active", async (Guid lotId, HttpContext httpContext, [FromBody] SetActiveBody body, IMediator mediator) =>
        {
            var key = EndpointIdempotency.ReadKey(httpContext);
            return (await mediator.Send(new SetProductLotActiveCommand(lotId, body.IsActive, key))).ToHttpResult();
        });

        group.MapGet("/suppliers", async ([FromQuery] bool? activeOnly, IMediator mediator) =>
            (await mediator.Send(new ListSuppliersQuery(activeOnly ?? true))).ToHttpResult());

        group.MapGet("/suppliers/{id:guid}", async (Guid id, IMediator mediator) =>
            (await mediator.Send(new GetSupplierByIdQuery(id))).ToHttpResult());

        group.MapPost("/suppliers", async (HttpContext httpContext, [FromBody] RegisterSupplierCommand command, IMediator mediator) =>
        {
            var key = EndpointIdempotency.ReadKey(httpContext);
            return (await mediator.Send(command with { IdempotencyKey = key })).ToHttpResult();
        });

        group.MapPut("/suppliers/{id:guid}", async (Guid id, HttpContext httpContext, [FromBody] UpdateSupplierBody body, IMediator mediator) =>
        {
            var key = EndpointIdempotency.ReadKey(httpContext);
            return (await mediator.Send(new UpdateSupplierCommand(id, body.LegalName, body.TradeName, body.ContactEmail, body.ContactPhone, key))).ToHttpResult();
        });

        group.MapPost("/suppliers/{id:guid}/active", async (Guid id, HttpContext httpContext, [FromBody] SetActiveBody body, IMediator mediator) =>
        {
            var key = EndpointIdempotency.ReadKey(httpContext);
            return (await mediator.Send(new SetSupplierActiveCommand(id, body.IsActive, key))).ToHttpResult();
        });

        group.MapPost("/stock/movements", async ([FromBody] RegisterStockMovementCommand command, IMediator mediator) =>
            (await mediator.Send(command)).ToHttpResult());

        return app;
    }

    private sealed record SetActiveBody(bool IsActive);

    private sealed record UpdateProductBody(
        string Name,
        string Description,
        string Sku,
        string Barcode,
        string UnitOfMeasure,
        decimal ReorderLevel,
        Inventory.Domain.Enums.ProductCategory Category,
        string Ncm,
        string? Cest,
        int MerchandiseOrigin,
        Guid? SupplierId,
        bool RequiresLot);

    private sealed record RegisterProductLotBody(
        string LotNumber,
        DateTimeOffset? ExpirationDate,
        decimal UnitCost,
        decimal InitialQuantity,
        Guid LotId = default);

    private sealed record UpdateProductLotBody(DateTimeOffset? ExpirationDate, decimal UnitCost);

    private sealed record UpdateSupplierBody(string LegalName, string TradeName, string? ContactEmail, string? ContactPhone);
}
