using Core.Domain;
using MediatR;
using Core.Application.IntegrationEvents;
using Sales.Domain.Repositories;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
            return Result.Failure<bool>(new Error("Order.NotFound", "Pedido não encontrado."));
        }

        var payResult = order.Pay();
        if (!payResult.IsSuccess)
        {
            return payResult;
        }

        _orderRepository.Update(order);

        // Dispara evento de integração para baixar o estoque (desacoplado)
        var items = order.Items.Select(i => new OrderPaidItem(i.ProductId, i.Quantity)).ToList();
        var domainEvent = new OrderPaidEvent(order.Id, items);
        
        // Em um ambiente real com Outbox Pattern e Message Broker, isso seria salvo na mesma transação.
        // Aqui estamos publicando em memória, mas o TransactionBehavior garantirá o commit.
        await _publisher.Publish(domainEvent, cancellationToken);

        return Result.Success(true);
    }
}
