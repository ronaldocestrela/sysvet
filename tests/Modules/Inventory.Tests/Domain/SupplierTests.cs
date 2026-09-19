using FluentAssertions;
using Inventory.Domain.Entities;
using Xunit;

namespace Inventory.Tests.Domain;

public class SupplierTests
{
    [Fact]
    public void Create_WithValidCnpj_ReturnsSuccess()
    {
        var result = Supplier.Create("Distribuidora Vet Ltda", "Vet Dist", "12.345.678/0001-95", "a@b.com", "11999999999");
        result.IsSuccess.Should().BeTrue();
        result.Value.Document.Should().Be("12345678000195");
    }

    [Fact]
    public void Create_WithInvalidCnpj_ReturnsFailure()
    {
        var result = Supplier.Create("X", "X", "123", null, null);
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Update_AndSetActive_MutateSupplier()
    {
        var supplier = Supplier.Create("Distribuidora Vet Ltda", "Vet Dist", "12.345.678/0001-95", "a@b.com", "11999999999").Value;

        supplier.Update("Nova Razao", "Nova", "b@c.com", "11888888888").IsSuccess.Should().BeTrue();
        supplier.LegalName.Should().Be("Nova Razao");
        supplier.SetActive(false);
        supplier.IsActive.Should().BeFalse();
    }
}
