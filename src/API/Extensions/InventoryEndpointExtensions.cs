using Inventory.Application.Products.Commands;
using Inventory.Application.StockMovements.Commands;
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
            .WithTags("Inventory")
            .RequireAuthorization();

        group.MapPost("/products", async ([FromBody] RegisterProductCommand command, IMediator mediator) =>
            (await mediator.Send(command)).ToHttpResult());

        group.MapPost("/stock/movements", async ([FromBody] RegisterStockMovementCommand command, IMediator mediator) =>
            (await mediator.Send(command)).ToHttpResult());

        return app;
    }
}
