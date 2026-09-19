using Core.Domain;
using MediatR;
using Sales.Application.Orders.Dtos;
using Sales.Domain.Repositories;

namespace Sales.Application.Orders.Queries;

public sealed class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, Result<OrderDetailDto>>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderByIdQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<OrderDetailDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure<OrderDetailDto>(Sales.Domain.ErrorCodes.Order.NotFound);
        }

        var dto = new OrderDetailDto
        {
            Id = order.Id,
            CashRegisterId = order.CashRegisterId,
            Status = order.Status,
            TutorId = order.TutorId,
            PetId = order.PetId,
            SourceQuoteId = order.SourceQuoteId,
            FinanceIntegrationStatus = order.FinanceIntegrationStatus,
            CreatedAt = order.CreatedAt,
            PaidAt = order.PaidAt,
            SubtotalAmount = order.SubtotalAmount,
            DiscountPercent = order.DiscountPercent,
            DiscountAmount = order.DiscountAmount,
            TotalAmount = order.TotalAmount.Amount,
            Items = order.Items.Select(i => new OrderItemDetailDto
            {
                Id = i.Id,
                Kind = i.Kind,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice.Amount,
                LineTotal = i.TotalPrice.Amount,
                ReturnedQuantity = i.ReturnedQuantity,
                RemainingQuantity = i.RemainingQuantity
            }).ToList(),
            Commissions = order.Commissions.Select(c => new OrderCommissionDetailDto
            {
                Id = c.Id,
                OrderItemId = c.OrderItemId,
                PayeeUserId = c.PayeeUserId,
                Role = c.Role,
                RatePercent = c.RatePercent,
                CommissionAmount = c.CommissionAmount.Amount,
                Status = c.Status
            }).ToList(),
            Payments = order.Payments.Select(p => new OrderPaymentDetailDto
            {
                Id = p.Id,
                Method = p.Method,
                Amount = p.Amount.Amount,
                Nsu = p.Nsu,
                AuthorizationCode = p.AuthorizationCode,
                Provider = p.Provider,
                Installments = p.Installments,
                RemainingRefundable = p.RemainingRefundable,
                Refunds = p.Refunds.Select(r => new OrderPaymentRefundDetailDto
                {
                    Id = r.Id,
                    Amount = r.Amount.Amount,
                    RefundNsu = r.RefundNsu,
                    CreatedAt = r.CreatedAt
                }).ToList()
            }).ToList()
        };

        return Result.Success(dto);
    }
}
