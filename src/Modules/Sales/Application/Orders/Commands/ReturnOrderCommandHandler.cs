using Core.Application.IntegrationEvents;
using Core.Domain;
using MediatR;
using Sales.Domain.Entities;
using Sales.Domain.Enums;
using Sales.Domain.Payments;
using Sales.Domain.Repositories;
using Sales.Domain.Services;

namespace Sales.Application.Orders.Commands;

public sealed class ReturnOrderCommandHandler : IRequestHandler<ReturnOrderCommand, Result<Guid>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductKitRepository _productKitRepository;
    private readonly IServicePackageRepository _servicePackageRepository;
    private readonly IPrepaidBalanceRepository _prepaidBalanceRepository;
    private readonly IPaymentTerminal _paymentTerminal;
    private readonly IPublisher _publisher;
    private readonly IMediator _mediator;

    public ReturnOrderCommandHandler(
        IOrderRepository orderRepository,
        IProductKitRepository productKitRepository,
        IServicePackageRepository servicePackageRepository,
        IPrepaidBalanceRepository prepaidBalanceRepository,
        IPaymentTerminal paymentTerminal,
        IPublisher publisher,
        IMediator mediator)
    {
        _orderRepository = orderRepository;
        _productKitRepository = productKitRepository;
        _servicePackageRepository = servicePackageRepository;
        _prepaidBalanceRepository = prepaidBalanceRepository;
        _paymentTerminal = paymentTerminal;
        _publisher = publisher;
        _mediator = mediator;
    }

    public async Task<Result<Guid>> Handle(ReturnOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure<Guid>(Sales.Domain.ErrorCodes.Order.NotFound);
        }

        var existingReturn = order.Returns.FirstOrDefault(r => r.Id == request.ReturnId);
        if (existingReturn is not null)
        {
            return Result.Success(existingReturn.Id);
        }

        var kitIds = order.Items
            .Where(i => i.Kind == OrderItemKind.Kit && i.CatalogOfferId.HasValue)
            .Select(i => i.CatalogOfferId!.Value)
            .Distinct()
            .ToList();
        var kitsById = new Dictionary<Guid, Sales.Domain.Entities.ProductKit>();
        foreach (var kitId in kitIds)
        {
            var kit = await _productKitRepository.GetByIdAsync(kitId, cancellationToken);
            if (kit is not null)
            {
                kitsById[kitId] = kit;
            }
        }

        var stockLines = new List<RestoreStockForSaleReturnLine>();
        foreach (var line in request.Lines)
        {
            var item = order.Items.FirstOrDefault(i => i.Id == line.OrderItemId);
            if (item is null)
            {
                continue;
            }

            if (item is { Kind: OrderItemKind.Product, ProductId: not null })
            {
                stockLines.Add(new RestoreStockForSaleReturnLine(item.ProductId.Value, line.Quantity));
                continue;
            }

            if (item is { Kind: OrderItemKind.Kit, CatalogOfferId: not null } &&
                kitsById.TryGetValue(item.CatalogOfferId.Value, out var kit))
            {
                foreach (var (productId, qty) in kit.ExplodeStockLines(line.Quantity))
                {
                    stockLines.Add(new RestoreStockForSaleReturnLine(productId, qty));
                }
            }
        }

        if (stockLines.Count > 0)
        {
            var restore = await _mediator.Send(
                new RestoreStockForSaleReturnRequest(order.Id, request.ReturnId, stockLines),
                cancellationToken);
            if (restore.IsFailure)
            {
                return Result.Failure<Guid>(restore.Error);
            }
        }

        var returnLines = request.Lines.Select(l => (l.OrderItemId, l.Quantity)).ToList();
        var returnResult = order.ReturnItems(request.ReturnId, returnLines);
        if (returnResult.IsFailure)
        {
            return Result.Failure<Guid>(returnResult.Error);
        }

        var saleReturn = returnResult.Value;

        foreach (var line in request.Lines)
        {
            var item = order.Items.FirstOrDefault(i => i.Id == line.OrderItemId);
            if (item is not { Kind: OrderItemKind.Package, CatalogOfferId: not null })
            {
                continue;
            }

            var package = await _servicePackageRepository.GetByIdAsync(item.CatalogOfferId.Value, cancellationToken);
            if (package is null)
            {
                continue;
            }

            var usesToReverse = (int)(line.Quantity * package.UsesPerUnit);
            if (usesToReverse <= 0)
            {
                continue;
            }

            var balance = await _prepaidBalanceRepository.GetByPetAndServiceAsync(order.PetId!.Value, package.ServiceCode, cancellationToken);
            if (balance is null)
            {
                continue;
            }

            var reverse = balance.ReverseCredit(item.Id, usesToReverse);
            if (reverse.IsFailure)
            {
                return Result.Failure<Guid>(reverse.Error);
            }

        }

        var refundRemaining = saleReturn.RefundAmount.Amount;
        if (refundRemaining > 0)
        {
            foreach (var payment in order.Payments.OrderByDescending(p => p.Method == PaymentMethod.Cash))
            {
                if (refundRemaining <= 0)
                {
                    break;
                }

                var slice = Math.Min(refundRemaining, payment.RemainingRefundable);
                if (slice <= 0)
                {
                    continue;
                }

                var refundNsu = (string?)null;
                if (Payment.RequiresTefNsu(payment.Method) && string.IsNullOrWhiteSpace(refundNsu))
                {
                    if (string.IsNullOrWhiteSpace(payment.Nsu))
                    {
                        return Result.Failure<Guid>(Sales.Domain.ErrorCodes.Payment.NsuRequired);
                    }

                    var terminalResult = await _paymentTerminal.RefundAsync(
                        new PaymentTerminalRefundRequest(payment.Method, slice, payment.Nsu),
                        cancellationToken);
                    if (terminalResult.IsFailure)
                    {
                        return Result.Failure<Guid>(terminalResult.Error);
                    }

                    refundNsu = terminalResult.Value.RefundNsu;
                }

                var refundResult = order.RefundPayment(payment.Id, slice, refundNsu);
                if (refundResult.IsFailure)
                {
                    return Result.Failure<Guid>(refundResult.Error);
                }

                await _orderRepository.PersistRefundAsync(
                    order.Id,
                    refundResult.Value,
                    order.Status,
                    order.UpdatedAt,
                    cancellationToken);

                refundRemaining -= slice;
            }
        }

        _orderRepository.Update(order);

        await _publisher.Publish(
            new OrderReturnedEvent(order.Id, saleReturn.Id, saleReturn.RefundAmount.Amount, order.Status.ToString()),
            cancellationToken);

        return Result.Success(saleReturn.Id);
    }
}
