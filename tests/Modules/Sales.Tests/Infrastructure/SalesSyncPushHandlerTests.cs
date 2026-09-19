using Core.Application.Sync;
using Core.Domain;
using FluentAssertions;
using MediatR;
using NSubstitute;
using Sales.Application.Orders.Commands;
using Sales.Domain.Enums;
using Sales.Infrastructure.Sync;

namespace Sales.Tests.Infrastructure;

public class SalesSyncPushHandlerTests
{
    [Fact]
    public void TryMapCommand_CreateOrder_ReturnsCommandWithIdempotency()
    {
        var handler = new SalesSyncPushHandler(Substitute.For<IMediator>());
        var key = Guid.NewGuid();
        var payload = System.Text.Json.JsonSerializer.Serialize(new CreateOrderCommand
        {
            OrderId = Guid.NewGuid(),
            CashRegisterId = Guid.NewGuid(),
            Items =
            [
                new CreateOrderItemDto
                {
                    Kind = OrderItemKind.Product,
                    ProductId = Guid.NewGuid(),
                    ProductName = "P",
                    Quantity = 1,
                    UnitPrice = 1
                }
            ]
        });

        var command = handler.TryMapCommand(new SyncOutboxMessageDto
        {
            Id = key,
            Type = nameof(CreateOrderCommand),
            Payload = payload,
            CreatedAt = DateTimeOffset.UtcNow
        }) as CreateOrderCommand;

        command.Should().NotBeNull();
        command!.IdempotencyKey.Should().Be(key);
    }
}
