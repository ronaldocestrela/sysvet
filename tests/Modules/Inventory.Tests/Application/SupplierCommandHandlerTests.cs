using Core.Domain;
using Core.Domain.Auditing;
using FluentAssertions;
using Inventory.Application.Suppliers.Commands;
using Inventory.Domain.Entities;
using Inventory.Domain.Repositories;
using NSubstitute;

namespace Inventory.Tests.Application;

public class SupplierCommandHandlerTests
{
    [Fact]
    public async Task Update_WhenSupplierExists_UpdatesFields()
    {
        var supplier = Supplier.Create("Razao", "Fantasia", "12.345.678/0001-95", "a@b.com", "1199").Value;
        var repo = Substitute.For<ISupplierRepository>();
        var tenant = Substitute.For<ITenantContext>();
        var audit = Substitute.For<IAuditLogger>();
        tenant.TenantId.Returns(Guid.NewGuid());
        tenant.UserId.Returns(Guid.NewGuid());
        repo.GetByIdAsync(supplier.Id, Arg.Any<CancellationToken>()).Returns(supplier);

        var handler = new UpdateSupplierCommandHandler(repo, tenant, audit);
        var result = await handler.Handle(new UpdateSupplierCommand(supplier.Id, "Nova", "N", "c@d.com", "1188"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        supplier.LegalName.Should().Be("Nova");
        repo.Received(1).Update(supplier);
    }

    [Fact]
    public async Task Update_WhenMissing_ReturnsNotFound()
    {
        var repo = Substitute.For<ISupplierRepository>();
        repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Supplier?)null);
        var handler = new UpdateSupplierCommandHandler(repo, Substitute.For<ITenantContext>(), Substitute.For<IAuditLogger>());

        var result = await handler.Handle(new UpdateSupplierCommand(Guid.NewGuid(), "X", "X", null, null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Supplier.NotFound");
    }

    [Fact]
    public async Task SetActive_DeactivatesSupplier()
    {
        var supplier = Supplier.Create("Razao", "Fantasia", "12.345.678/0001-95", null, null).Value;
        var repo = Substitute.For<ISupplierRepository>();
        var tenant = Substitute.For<ITenantContext>();
        var audit = Substitute.For<IAuditLogger>();
        tenant.TenantId.Returns(Guid.NewGuid());
        tenant.UserId.Returns(Guid.NewGuid());
        repo.GetByIdAsync(supplier.Id, Arg.Any<CancellationToken>()).Returns(supplier);

        var handler = new SetSupplierActiveCommandHandler(repo, tenant, audit);
        var result = await handler.Handle(new SetSupplierActiveCommand(supplier.Id, false), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        supplier.IsActive.Should().BeFalse();
    }
}
