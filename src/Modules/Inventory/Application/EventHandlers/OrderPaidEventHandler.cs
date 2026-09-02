using Core.Application.IntegrationEvents;
using Inventory.Domain.Entities;
using Inventory.Domain.Repositories;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Inventory.Application.EventHandlers;

public class OrderPaidEventHandler : INotificationHandler<OrderPaidEvent>
{
    private readonly IStockMovementRepository _stockMovementRepository;

    public OrderPaidEventHandler(IStockMovementRepository stockMovementRepository)
    {
        _stockMovementRepository = stockMovementRepository;
    }

    public async Task Handle(OrderPaidEvent notification, CancellationToken cancellationToken)
    {
        foreach (var item in notification.Items)
        {
            // Cria uma movimentação de saída para cada item pago
            var reason = $"Venda - Pedido {notification.OrderId}";
            var movementResult = StockMovement.Create(item.ProductId, MovementType.Out, item.Quantity, null, null, reason);
            
            if (movementResult.IsSuccess)
            {
                _stockMovementRepository.Add(movementResult.Value);
            }
        }

        // Em uma implementação real com banco de dados de alta concorrência e Outbox,
        // o SaveChanges seria feito pelo TransactionBehavior na mesma transação.
        // O UoW cuida disso via Pipeline Behavior do MediatR.
        await Task.CompletedTask;
    }
}
