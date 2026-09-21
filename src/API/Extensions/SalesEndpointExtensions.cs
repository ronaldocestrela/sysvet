using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Sales.Application.CashRegisters.Commands;
using Sales.Application.CashRegisters.Queries;
using Sales.Application.Catalog;
using Sales.Application.Commissions;
using Sales.Application.Orders.Commands;
using Sales.Application.Orders.Queries;
using Sales.Application.Prepaid;

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

        group.MapPost("/cash-registers/close", async (HttpContext httpContext, CloseCashRegisterCommand command, IMediator mediator) =>
        {
            command.IdempotencyKey = EndpointIdempotency.ReadKey(httpContext);
            return (await mediator.Send(command)).ToHttpResult();
        });

        group.MapGet("/cash-registers/{id:guid}", async (Guid id, IMediator mediator) =>
            (await mediator.Send(new GetCashRegisterByIdQuery(id))).ToHttpResult());

        group.MapPost("/cash-registers/{id:guid}/movements", async (
            HttpContext httpContext,
            Guid id,
            RecordCashMovementCommand command,
            IMediator mediator) =>
        {
            command.CashRegisterId = id;
            command.IdempotencyKey = EndpointIdempotency.ReadKey(httpContext);
            return (await mediator.Send(command)).ToHttpResult();
        });

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

        group.MapPost("/orders/{orderId:guid}/returns", async (
            HttpContext httpContext,
            Guid orderId,
            ReturnOrderCommand command,
            IMediator mediator) =>
        {
            command.OrderId = orderId;
            command.IdempotencyKey = EndpointIdempotency.ReadKey(httpContext);
            return (await mediator.Send(command)).ToHttpResult();
        });

        group.MapGet("/commission-rules", async (IMediator mediator) =>
            (await mediator.Send(new ListCommissionRulesQuery())).ToHttpResult());

        group.MapPut("/commission-rules", async (UpsertCommissionRuleCommand command, IMediator mediator) =>
            (await mediator.Send(command)).ToHttpResult());

        group.MapGet("/commissions", async (Guid? orderId, IMediator mediator) =>
            (await mediator.Send(new ListCommissionAccrualsQuery(orderId))).ToHttpResult());

        group.MapGet("/product-kits", async (IMediator mediator) =>
            (await mediator.Send(new ListProductKitsQuery())).ToHttpResult());

        group.MapPut("/product-kits", async (UpsertProductKitCommand command, IMediator mediator) =>
            (await mediator.Send(command)).ToHttpResult());

        group.MapGet("/service-packages", async (IMediator mediator) =>
            (await mediator.Send(new ListServicePackagesQuery())).ToHttpResult());

        group.MapPut("/service-packages", async (UpsertServicePackageCommand command, IMediator mediator) =>
            (await mediator.Send(command)).ToHttpResult());

        group.MapGet("/prepaid-balances", async (Guid? tutorId, Guid? petId, Sales.Domain.Enums.ServiceCode? serviceCode, IMediator mediator) =>
            (await mediator.Send(new ListPrepaidBalancesQuery(tutorId, petId, serviceCode))).ToHttpResult());

        group.MapPost("/prepaid-balances/consume", async (HttpContext httpContext, ConsumePrepaidPackageUseCommand command, IMediator mediator) =>
        {
            command.IdempotencyKey = EndpointIdempotency.ReadKey(httpContext);
            return (await mediator.Send(command)).ToHttpResult();
        });

        group.MapPost("/orders/{orderId:guid}/payments/{paymentId:guid}/refund", async (
            HttpContext httpContext,
            Guid orderId,
            Guid paymentId,
            RefundOrderPaymentCommand command,
            IMediator mediator) =>
        {
            command.OrderId = orderId;
            command.PaymentId = paymentId;
            command.IdempotencyKey = EndpointIdempotency.ReadKey(httpContext);
            return (await mediator.Send(command)).ToHttpResult();
        });

        return endpoints;
    }
}
