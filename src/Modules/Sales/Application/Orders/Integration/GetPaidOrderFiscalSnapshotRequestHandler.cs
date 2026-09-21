using Core.Application.IntegrationEvents;
using Core.Domain;
using MediatR;
using Sales.Domain.Enums;
using Sales.Domain.Repositories;

namespace Sales.Application.Orders.Integration;

/// <summary>Returns paid order lines for fiscal module.</summary>
public sealed class GetPaidOrderFiscalSnapshotRequestHandler
    : IRequestHandler<GetPaidOrderFiscalSnapshotRequest, Result<PaidOrderFiscalSnapshot>>
{
    private readonly IOrderRepository _orderRepository;

    public GetPaidOrderFiscalSnapshotRequestHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<PaidOrderFiscalSnapshot>> Handle(
        GetPaidOrderFiscalSnapshotRequest request,
        CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure<PaidOrderFiscalSnapshot>(Sales.Domain.ErrorCodes.Order.NotFound);
        }

        var lines = order.Items.Select(i => new PaidOrderFiscalLine
        {
            Kind = i.Kind.ToString(),
            ProductId = i.ProductId,
            Description = i.ProductName,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice.Amount
        }).ToList();

        return Result.Success(new PaidOrderFiscalSnapshot
        {
            OrderId = order.Id,
            TutorId = order.TutorId,
            IsPaid = order.Status == OrderStatus.Paid,
            Lines = lines
        });
    }
}
