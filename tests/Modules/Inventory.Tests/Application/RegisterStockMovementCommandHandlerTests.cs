using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Domain;
using Core.Domain.Auditing;
using FluentAssertions;
using Inventory.Application.StockMovements.Commands;
using Inventory.Domain.Entities;
using Inventory.Domain.Repositories;
using NSubstitute;
using Xunit;

namespace Inventory.Tests.Application;

public class RegisterStockMovementCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidInMovement_ReturnsSuccessAndUpdatesBalance()
    {
        // Arrange
        var productRepository = Substitute.For<IProductRepository>();
        var movementRepository = Substitute.For<IStockMovementRepository>();
        var unitOfWork = Substitute.For<IInventoryUnitOfWork>();
        var tenantContext = Substitute.For<ITenantContext>();
        var auditLogger = Substitute.For<IAuditLogger>();

        tenantContext.TenantId.Returns(Guid.NewGuid());
        tenantContext.UserId.Returns(Guid.NewGuid());

        var productId = Guid.NewGuid();
        var product = Product.Create("P", "D", "B", "U", 0).Value;
        
        // Simular que o produto existe
        productRepository.GetByIdAsync(productId, Arg.Any<CancellationToken>()).Returns(product);
        
        // Simular balance existente
        var balance = new ProductBalance(productId, 10m);
        productRepository.GetBalanceAsync(productId, Arg.Any<CancellationToken>()).Returns(balance);

        var handler = new RegisterStockMovementCommandHandler(productRepository, movementRepository, unitOfWork, tenantContext, auditLogger);
        var command = new RegisterStockMovementCommand(productId, MovementType.In, 5m, "L1", null, "Compra");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        balance.TotalQuantity.Should().Be(15m); // 10 + 5
        movementRepository.Received(1).Add(Arg.Any<StockMovement>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidOutMovement_InsufficientFunds_ReturnsFailure()
    {
        // Arrange
        var productRepository = Substitute.For<IProductRepository>();
        var movementRepository = Substitute.For<IStockMovementRepository>();
        var unitOfWork = Substitute.For<IInventoryUnitOfWork>();
        var tenantContext = Substitute.For<ITenantContext>();
        var auditLogger = Substitute.For<IAuditLogger>();

        var productId = Guid.NewGuid();
        var product = Product.Create("P", "D", "B", "U", 0).Value;
        
        productRepository.GetByIdAsync(productId, Arg.Any<CancellationToken>()).Returns(product);
        
        // Saldo inicial é 10
        var balance = new ProductBalance(productId, 10m);
        productRepository.GetBalanceAsync(productId, Arg.Any<CancellationToken>()).Returns(balance);

        var handler = new RegisterStockMovementCommandHandler(productRepository, movementRepository, unitOfWork, tenantContext, auditLogger);
        
        // Tentar remover 15 (falta saldo)
        var command = new RegisterStockMovementCommand(productId, MovementType.Out, 15m, null, null, "Venda");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ProductBalance.InsufficientFunds");
    }
}
