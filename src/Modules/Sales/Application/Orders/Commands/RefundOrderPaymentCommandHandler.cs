using Core.Application.IntegrationEvents;
using Core.Domain;
using MediatR;
using Sales.Domain.Entities;
using Sales.Domain.Enums;
using Sales.Domain.Payments;
using Sales.Domain.Repositories;

namespace Sales.Application.Orders.Commands;

public sealed class RefundOrderPaymentCommandHandler : IRequestHandler<RefundOrderPaymentCommand, Result<Guid>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentTerminal _paymentTerminal;
    private readonly IPublisher _publisher;

    public RefundOrderPaymentCommandHandler(
        IOrderRepository orderRepository,
        IPaymentTerminal paymentTerminal,
        IPublisher publisher)
    {
        _orderRepository = orderRepository;
        _paymentTerminal = paymentTerminal;
        _publisher = publisher;
    }

    public async Task<Result<Guid>> Handle(RefundOrderPaymentCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure<Guid>(Sales.Domain.ErrorCodes.Order.NotFound);
        }

        var payment = order.Payments.FirstOrDefault(p => p.Id == request.PaymentId);
        if (payment is null)
        {
            return Result.Failure<Guid>(Sales.Domain.ErrorCodes.Payment.NotFound);
        }

        var refundNsu = request.RefundNsu;
        if (Payment.RequiresTefNsu(payment.Method) && string.IsNullOrWhiteSpace(refundNsu))
        {
            if (string.IsNullOrWhiteSpace(payment.Nsu))
            {
                return Result.Failure<Guid>(Sales.Domain.ErrorCodes.Payment.NsuRequired);
            }

            var terminalResult = await _paymentTerminal.RefundAsync(
                new PaymentTerminalRefundRequest(payment.Method, request.Amount, payment.Nsu),
                cancellationToken);
            if (terminalResult.IsFailure)
            {
                return Result.Failure<Guid>(terminalResult.Error);
            }

            refundNsu = terminalResult.Value.RefundNsu;
        }

        var refundResult = order.RefundPayment(request.PaymentId, request.Amount, refundNsu);
        if (refundResult.IsFailure)
        {
            return Result.Failure<Guid>(refundResult.Error);
        }

        var refund = refundResult.Value;
        await _orderRepository.PersistRefundAsync(
            order.Id,
            refund,
            order.Status,
            order.UpdatedAt,
            cancellationToken);

        await _publisher.Publish(
            new OrderPaymentRefundedEvent(
                order.Id,
                payment.Id,
                refund.Id,
                payment.Method.ToString(),
                refund.Amount.Amount,
                refund.RefundNsu,
                order.Status.ToString()),
            cancellationToken);

        return Result.Success(refund.Id);
    }
}
