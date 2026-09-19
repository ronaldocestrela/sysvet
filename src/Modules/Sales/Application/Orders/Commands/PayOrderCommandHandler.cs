using Core.Application.IntegrationEvents;
using Core.Domain;
using MediatR;
using Sales.Domain.Entities;
using Sales.Domain.Enums;
using Sales.Domain.Payments;
using Sales.Domain.Repositories;
using Sales.Domain.Services;

namespace Sales.Application.Orders.Commands;

public class PayOrderCommandHandler : IRequestHandler<PayOrderCommand, Result<bool>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICommissionRuleRepository _commissionRuleRepository;
    private readonly IPaymentTerminal _paymentTerminal;
    private readonly IPublisher _publisher;
    private readonly IMediator _mediator;

    public PayOrderCommandHandler(
        IOrderRepository orderRepository,
        ICommissionRuleRepository commissionRuleRepository,
        IPaymentTerminal paymentTerminal,
        IPublisher publisher,
        IMediator mediator)
    {
        _orderRepository = orderRepository;
        _commissionRuleRepository = commissionRuleRepository;
        _paymentTerminal = paymentTerminal;
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

        if (order.Status is OrderStatus.Paid or OrderStatus.PartiallyRefunded or OrderStatus.PartiallyReturned or OrderStatus.Returned or OrderStatus.Refunded)
        {
            return Result.Success(true);
        }

        var buildResult = await BuildPaymentEntitiesAsync(request.Payments, cancellationToken);
        if (buildResult.IsFailure)
        {
            return Result.Failure<bool>(buildResult.Error);
        }

        var (paymentEntities, authorizedForCompensation) = buildResult.Value;

        var payResult = order.Pay(paymentEntities);
        if (!payResult.IsSuccess)
        {
            await CompensateAuthorizationsAsync(authorizedForCompensation, cancellationToken);
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
                await CompensateAuthorizationsAsync(authorizedForCompensation, cancellationToken);
                return Result.Failure<bool>(Sales.Domain.ErrorCodes.Order.InsufficientStock);
            }
        }

        if (!order.Commissions.Any())
        {
            var rules = await _commissionRuleRepository.ListAllAsync(cancellationToken);
            var accruals = CommissionCalculator.Calculate(order, order.SellerUserId, rules);
            order.AttachCommissions(accruals);
        }

        _orderRepository.Update(order);

        var items = order.Items
            .Select(i => new OrderPaidItem(i.ProductId, i.Quantity, i.Kind.ToString()))
            .ToList();
        var payments = order.Payments
            .Select(p => new OrderPaidPayment(p.Method.ToString(), p.Amount.Amount, p.Nsu))
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

    private async Task<Result<(List<Payment> Payments, List<PaymentTerminalRefundRequest> Authorized)>> BuildPaymentEntitiesAsync(
        IReadOnlyList<PayOrderPaymentDto> dtos,
        CancellationToken cancellationToken)
    {
        var paymentEntities = new List<Payment>();
        var authorizedForCompensation = new List<PaymentTerminalRefundRequest>();

        foreach (var dto in dtos)
        {
            var nsu = dto.Nsu;
            var authorizationCode = dto.AuthorizationCode;
            var provider = dto.Provider;
            var terminalId = dto.TerminalId;
            var brand = dto.Brand;
            var installments = dto.Installments;

            if (Payment.RequiresTefNsu(dto.Method) && string.IsNullOrWhiteSpace(nsu))
            {
                var authResult = await _paymentTerminal.AuthorizeAsync(
                    new PaymentTerminalAuthorizeRequest(dto.Method, dto.Amount, installments),
                    cancellationToken);
                if (authResult.IsFailure)
                {
                    await CompensateAuthorizationsAsync(authorizedForCompensation, cancellationToken);
                    return Result.Failure<(List<Payment>, List<PaymentTerminalRefundRequest>)>(authResult.Error);
                }

                var auth = authResult.Value;
                nsu = auth.Nsu;
                authorizationCode = auth.AuthorizationCode;
                provider = auth.Provider;
                terminalId = auth.TerminalId;
                brand = auth.Brand;
                installments = auth.Installments;
                authorizedForCompensation.Add(new PaymentTerminalRefundRequest(dto.Method, dto.Amount, auth.Nsu));
            }

            var created = Payment.Create(
                dto.Method,
                dto.Amount,
                nsu,
                authorizationCode,
                provider,
                terminalId,
                brand,
                installments);
            if (created.IsFailure)
            {
                await CompensateAuthorizationsAsync(authorizedForCompensation, cancellationToken);
                return Result.Failure<(List<Payment>, List<PaymentTerminalRefundRequest>)>(created.Error);
            }

            paymentEntities.Add(created.Value);
        }

        return Result.Success((paymentEntities, authorizedForCompensation));
    }

    private async Task CompensateAuthorizationsAsync(
        IReadOnlyList<PaymentTerminalRefundRequest> authorized,
        CancellationToken cancellationToken)
    {
        foreach (var item in authorized)
        {
            await _paymentTerminal.RefundAsync(item, cancellationToken);
        }
    }
}
