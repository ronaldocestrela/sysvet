using Core.Domain;
using Core.Domain.Auditing;
using FluentAssertions;
using Inventory.Application.Products.Commands;
using Inventory.Domain.Entities;
using Inventory.Domain.Repositories;
using NSubstitute;

namespace Inventory.Tests.Application;

public class RegisterProductCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidCommand_ReturnsSuccess()
    {
        var productRepository = Substitute.For<IProductRepository>();
        var tenantContext = Substitute.For<ITenantContext>();
        var auditLogger = Substitute.For<IAuditLogger>();

        tenantContext.TenantId.Returns(Guid.NewGuid());
        tenantContext.UserId.Returns(Guid.NewGuid());

        var handler = new RegisterProductCommandHandler(productRepository, tenantContext, auditLogger);
        var command = new RegisterProductCommand("Ração", "Premium", "123", "Kg", 5m);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        productRepository.Received(1).Add(Arg.Any<Product>());
        await auditLogger.Received(1).LogAsync(tenantContext.TenantId, tenantContext.UserId, Arg.Any<Guid>(), "Product", "Register", Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
