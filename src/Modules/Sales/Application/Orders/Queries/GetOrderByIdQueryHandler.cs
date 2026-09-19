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
            TotalAmount = order.TotalAmount.Amount,
            Items = order.Items.Select(i => new OrderItemDetailDto
            {
                Kind = i.Kind,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice.Amount,
                LineTotal = i.TotalPrice.Amount
            }).ToList(),
            Payments = order.Payments.Select(p => new OrderPaymentDetailDto
            {
                Method = p.Method,
                Amount = p.Amount.Amount
            }).ToList()
        };

        return Result.Success(dto);
    }
}
