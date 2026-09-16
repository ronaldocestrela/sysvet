using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Sales.Application.CashRegisters.Commands;
using Sales.Application.Orders.Commands;

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
        var group = endpoints.MapGroup("/api/v1/sales").RequireAuthorization();

        group.MapPost("/cash-registers/open", async (OpenCashRegisterCommand command, IMediator mediator) =>
            (await mediator.Send(command)).ToHttpResult());

        group.MapPost("/cash-registers/close", async (CloseCashRegisterCommand command, IMediator mediator) =>
            (await mediator.Send(command)).ToHttpResult());

        group.MapPost("/orders", async (CreateOrderCommand command, IMediator mediator) =>
            (await mediator.Send(command)).ToHttpResult());

        group.MapPost("/orders/{id}/pay", async (Guid id, IMediator mediator) =>
            (await mediator.Send(new PayOrderCommand { OrderId = id })).ToHttpResult());

        return endpoints;
    }
}
