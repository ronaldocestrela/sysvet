using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Sales.Application.CashRegisters.Commands;
using Sales.Application.CashRegisters.Queries;
using Sales.Application.Orders.Commands;
using Sales.Application.Orders.Queries;

namespace API.Extensions;

/// <summary>
/// Sales module minimal API endpoints.
/// </summary>
public static class SalesEndpointExtensions
{
    /// <summary>
    /// Maps PDV and cash register routes under <c>/api/v1/sales</c>.
    /// </summary>
    public static IEndpointRouteBuilder MapSalesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/sales")
            .RequireAuthorization()
            .WithTags("Sales");

        group.MapGet("/cash-registers/open", async (IMediator mediator) =>
            (await mediator.Send(new GetOpenCashRegisterQuery())).ToHttpResult());

        group.MapPost("/cash-registers/open", async (HttpContext httpContext, OpenCashRegisterCommand command, IMediator mediator) =>
        {
            command.IdempotencyKey = EndpointIdempotency.ReadKey(httpContext);
            return (await mediator.Send(command)).ToHttpResult();
        });

        group.MapPost("/cash-registers/close", async (CloseCashRegisterCommand command, IMediator mediator) =>
            (await mediator.Send(command)).ToHttpResult());

        group.MapPost("/orders", async (HttpContext httpContext, CreateOrderCommand command, IMediator mediator) =>
        {
            command.IdempotencyKey = EndpointIdempotency.ReadKey(httpContext);
            return (await mediator.Send(command)).ToHttpResult();
        });

        group.MapGet("/orders/{id:guid}", async (Guid id, IMediator mediator) =>
            (await mediator.Send(new GetOrderByIdQuery { OrderId = id })).ToHttpResult());

        group.MapPost("/orders/{id:guid}/pay", async (HttpContext httpContext, Guid id, PayOrderCommand command, IMediator mediator) =>
        {
            command.OrderId = id;
            command.IdempotencyKey = EndpointIdempotency.ReadKey(httpContext);
            return (await mediator.Send(command)).ToHttpResult();
        });

        return endpoints;
    }
}
