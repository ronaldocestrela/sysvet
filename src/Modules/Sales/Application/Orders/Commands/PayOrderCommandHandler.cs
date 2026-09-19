using Core.Application.IntegrationEvents;
using Core.Domain;
using MediatR;
using Sales.Domain.Entities;
using Sales.Domain.Enums;
using Sales.Domain.Repositories;

namespace Sales.Application.Orders.Commands;

public class PayOrderCommandHandler : IRequestHandler<PayOrderCommand, Result<bool>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPublisher _publisher;
    private readonly IMediator _mediator;

    public PayOrderCommandHandler(
        IOrderRepository orderRepository,
        IPublisher publisher,
        IMediator mediator)
    {
        _orderRepository = orderRepository;
        _publisher = publisher;
        _mediator = mediator;
    }

    public async Task<Result<bool>> Handle(PayOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
        {
            return Result.Failure<bool>(Sales.Domain.ErrorCodes.Order.NotFound);
        }

        var paymentEntities = new List<Payment>();
        foreach (var dto in request.Payments)
        {
            var created = Payment.Create(dto.Method, dto.Amount);
            if (created.IsFailure)
            {
                return Result.Failure<bool>(created.Error);
            }

            paymentEntities.Add(created.Value);
        }

        var payResult = order.Pay(paymentEntities);
        if (!payResult.IsSuccess)
        {
            return payResult;
        }

        var stockLines = order.Items
            .Where(i => i.Kind == OrderItemKind.Product && i.ProductId.HasValue)
            .Select(i => new ConsumeStockForSaleLine(i.ProductId!.Value, i.Quantity))
            .ToList();

        if (stockLines.Count > 0)
        {
            var stockResult = await _mediator.Send(new ConsumeStockForSaleRequest(order.Id, stockLines), cancellationToken);
            if (stockResult.IsFailure)
            {
                return Result.Failure<bool>(Sales.Domain.ErrorCodes.Order.InsufficientStock);
            }
        }

        _orderRepository.Update(order);

        var items = order.Items
            .Select(i => new OrderPaidItem(i.ProductId, i.Quantity, i.Kind.ToString()))
            .ToList();
        var payments = order.Payments
            .Select(p => new OrderPaidPayment(p.Method.ToString(), p.Amount.Amount))
            .ToList();

        await _publisher.Publish(
            new OrderPaidEvent(
                order.Id,
                order.TutorId,
                order.TotalAmount.Amount,
                order.FinanceIntegrationStatus.ToString(),
                items,
                payments),
            cancellationToken);

        if (order.SourceQuoteId.HasValue)
        {
            await _publisher.Publish(
                new ClinicalQuoteConvertedEvent(order.SourceQuoteId.Value, order.Id),
                cancellationToken);
        }

        return Result.Success(true);
    }
}
