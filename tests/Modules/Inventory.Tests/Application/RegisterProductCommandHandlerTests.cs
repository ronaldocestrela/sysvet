using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Domain;
using Core.Domain.Auditing;
using FluentAssertions;
using Inventory.Application.Products.Commands;
using Inventory.Domain.Entities;
using Inventory.Domain.Repositories;
using NSubstitute;
using Xunit;

namespace Inventory.Tests.Application;

public class RegisterProductCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidCommand_ReturnsSuccess()
    {
        // Arrange
        var productRepository = Substitute.For<IProductRepository>();
        var unitOfWork = Substitute.For<IInventoryUnitOfWork>();
        var tenantContext = Substitute.For<ITenantContext>();
        var auditLogger = Substitute.For<IAuditLogger>();

        tenantContext.TenantId.Returns(Guid.NewGuid());
        tenantContext.UserId.Returns(Guid.NewGuid());

        var handler = new RegisterProductCommandHandler(productRepository, unitOfWork, tenantContext, auditLogger);
        var command = new RegisterProductCommand("Ração", "Premium", "123", "Kg", 5m);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        productRepository.Received(1).Add(Arg.Any<Product>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await auditLogger.Received(1).LogAsync(tenantContext.TenantId, tenantContext.UserId, "Product", "Register", Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
