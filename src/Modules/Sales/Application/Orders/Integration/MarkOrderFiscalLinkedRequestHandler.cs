using Core.Application.IntegrationEvents;
using Core.Domain;
using MediatR;
using Sales.Domain.Repositories;

namespace Sales.Application.Orders.Integration;

/// <summary>Updates order fiscal integration status after emission.</summary>
public sealed class MarkOrderFiscalLinkedRequestHandler : IRequestHandler<MarkOrderFiscalLinkedRequest, Result>
{
    private readonly IOrderRepository _orderRepository;

    public MarkOrderFiscalLinkedRequestHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result> Handle(MarkOrderFiscalLinkedRequest request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure(Sales.Domain.ErrorCodes.Order.NotFound);
        }

        var result = order.MarkFiscalLinked(request.Partial);
        if (result.IsSuccess)
        {
            _orderRepository.Update(order);
        }

        return result;
    }
}
