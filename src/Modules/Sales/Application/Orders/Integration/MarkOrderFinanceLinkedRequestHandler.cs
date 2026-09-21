using Core.Application.IntegrationEvents;
using Core.Domain;
using MediatR;
using Sales.Domain.Repositories;

namespace Sales.Application.Orders.Integration;

/// <summary>
/// Updates order finance integration status after Finance materializes receivable.
/// </summary>
public sealed class MarkOrderFinanceLinkedRequestHandler : IRequestHandler<MarkOrderFinanceLinkedRequest, Result>
{
    private readonly IOrderRepository _orderRepository;

    public MarkOrderFinanceLinkedRequestHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result> Handle(MarkOrderFinanceLinkedRequest request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure(Sales.Domain.ErrorCodes.Order.NotFound);
        }

        var result = order.MarkFinanceLinked();
        if (result.IsSuccess)
        {
            _orderRepository.Update(order);
        }

        return result;
    }
}
