using Inventory.Application.Products.Commands;
using Inventory.Application.StockMovements.Commands;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace API.Endpoints.Inventory;

public static class InventoryEndpoints
{
    public static void MapInventoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/inventory")
            .WithTags("Inventory")
            .RequireAuthorization();

        group.MapPost("/products", async ([FromBody] RegisterProductCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            if (result.IsFailure)
            {
                return Results.BadRequest(result.Error);
            }
            return Results.Ok(result.Value);
        });

        group.MapPost("/stock/movements", async ([FromBody] RegisterStockMovementCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            if (result.IsFailure)
            {
                return Results.BadRequest(result.Error);
            }
            return Results.Ok(result.Value);
        });
    }
}
