using Core.Application.IntegrationEvents;
using Core.Domain;
using MediatR;
using Sales.Domain.Repositories;

namespace Sales.Application.Orders.Commands;

public class PayOrderCommandHandler : IRequestHandler<PayOrderCommand, Result<bool>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPublisher _publisher;

    public PayOrderCommandHandler(IOrderRepository orderRepository, IPublisher publisher)
    {
        _orderRepository = orderRepository;
        _publisher = publisher;
    }

    public async Task<Result<bool>> Handle(PayOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
        {
            return Result.Failure<bool>(Sales.Domain.ErrorCodes.Order.NotFound);
        }

        var payResult = order.Pay();
        if (!payResult.IsSuccess)
        {
            return payResult;
        }

        _orderRepository.Update(order);

        var items = order.Items.Select(i => new OrderPaidItem(i.ProductId, i.Quantity)).ToList();
        await _publisher.Publish(new OrderPaidEvent(order.Id, items), cancellationToken);

        return Result.Success(true);
    }
}
