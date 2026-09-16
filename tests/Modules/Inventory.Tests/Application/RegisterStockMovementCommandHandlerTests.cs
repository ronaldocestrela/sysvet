using Core.Domain;
using Core.Domain.Auditing;
using FluentAssertions;
using Inventory.Application.StockMovements.Commands;
using Inventory.Domain.Entities;
using Inventory.Domain.Repositories;
using NSubstitute;

namespace Inventory.Tests.Application;

public class RegisterStockMovementCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidInMovement_ReturnsSuccessAndUpdatesBalance()
    {
        var productRepository = Substitute.For<IProductRepository>();
        var movementRepository = Substitute.For<IStockMovementRepository>();
        var tenantContext = Substitute.For<ITenantContext>();
        var auditLogger = Substitute.For<IAuditLogger>();

        tenantContext.TenantId.Returns(Guid.NewGuid());
        tenantContext.UserId.Returns(Guid.NewGuid());

        var productId = Guid.NewGuid();
        var product = Product.Create("P", "D", "B", "U", 0).Value;

        productRepository.GetByIdAsync(productId, Arg.Any<CancellationToken>()).Returns(product);

        var balance = new ProductBalance(productId, 10m);
        productRepository.GetBalanceAsync(productId, Arg.Any<CancellationToken>()).Returns(balance);

        var handler = new RegisterStockMovementCommandHandler(productRepository, movementRepository, tenantContext, auditLogger);
        var command = new RegisterStockMovementCommand(productId, MovementType.In, 5m, "L1", null, "Compra");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        balance.TotalQuantity.Should().Be(15m);
        movementRepository.Received(1).Add(Arg.Any<StockMovement>());
    }

    [Fact]
    public async Task Handle_ValidOutMovement_InsufficientFunds_ReturnsFailure()
    {
        var productRepository = Substitute.For<IProductRepository>();
        var movementRepository = Substitute.For<IStockMovementRepository>();
        var tenantContext = Substitute.For<ITenantContext>();
        var auditLogger = Substitute.For<IAuditLogger>();

        var productId = Guid.NewGuid();
        var product = Product.Create("P", "D", "B", "U", 0).Value;

        productRepository.GetByIdAsync(productId, Arg.Any<CancellationToken>()).Returns(product);

        var balance = new ProductBalance(productId, 10m);
        productRepository.GetBalanceAsync(productId, Arg.Any<CancellationToken>()).Returns(balance);

        var handler = new RegisterStockMovementCommandHandler(productRepository, movementRepository, tenantContext, auditLogger);
        var command = new RegisterStockMovementCommand(productId, MovementType.Out, 15m, null, null, "Venda");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ProductBalance.InsufficientFunds");
    }
}
