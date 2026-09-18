using Core.Domain;
using Core.Domain.Auditing;
using FluentAssertions;
using Inventory.Application.Products.Commands;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Inventory.Domain.Repositories;
using NSubstitute;

namespace Inventory.Tests.Application;

public class RegisterProductCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidCommand_ReturnsSuccess()
    {
        var productRepository = Substitute.For<IProductRepository>();
        var supplierRepository = Substitute.For<ISupplierRepository>();
        var tenantContext = Substitute.For<ITenantContext>();
        var auditLogger = Substitute.For<IAuditLogger>();

        tenantContext.TenantId.Returns(Guid.NewGuid());
        tenantContext.UserId.Returns(Guid.NewGuid());
        productRepository.GetBySkuAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((Product?)null);
        productRepository.GetByBarcodeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((Product?)null);

        var handler = new RegisterProductCommandHandler(productRepository, supplierRepository, tenantContext, auditLogger);
        var command = new RegisterProductCommand(
            "Ração",
            "Premium",
            "RC-001",
            "7891234567890",
            "Kg",
            5m,
            ProductCategory.Food,
            "23091000",
            null,
            0,
            null,
            null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        productRepository.Received(1).Add(Arg.Any<Product>());
        await auditLogger.Received(1).LogAsync(tenantContext.TenantId, tenantContext.UserId, Arg.Any<Guid>(), "Product", "Register", Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
