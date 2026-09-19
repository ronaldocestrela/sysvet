using FluentAssertions;
using Inventory.Domain.ValueObjects;

namespace Inventory.Tests.Domain;

public class InventoryValueObjectTests
{
    [Fact]
    public void Money_Create_RejectsNegativeAndExposesZero()
    {
        Money.Create(-1m).IsFailure.Should().BeTrue();
        Money.Create(10m).Value.Amount.Should().Be(10m);
        Money.Zero.Amount.Should().Be(0m);
        Money.FromPersisted(3m).Amount.Should().Be(3m);
    }

    [Fact]
    public void Sku_AndBarcode_RejectInvalidInput()
    {
        Sku.Create(" ").IsFailure.Should().BeTrue();
        Sku.Create(new string('A', 41)).IsFailure.Should().BeTrue();
        Sku.Create("sku-1").Value.ToString().Should().Be("SKU-1");

        Barcode.Create(" ").IsFailure.Should().BeTrue();
        Barcode.Create("12").IsFailure.Should().BeTrue();
        Barcode.Create("1234").Value.ToString().Should().Be("1234");
    }

    [Fact]
    public void Ncm_Cnpj_AndAccessKey_ValidateDigitLength()
    {
        Ncm.Create(null).IsFailure.Should().BeTrue();
        Ncm.Create("123").IsFailure.Should().BeTrue();
        Ncm.Create("23091000").Value.Value.Should().Be("23091000");

        Cnpj.Create(" ").IsFailure.Should().BeTrue();
        Cnpj.Create("123").IsFailure.Should().BeTrue();
        Cnpj.Create("12.345.678/0001-95").Value.Value.Should().Be("12345678000195");

        AccessKey.Create(null).IsFailure.Should().BeTrue();
        AccessKey.Create("123").IsFailure.Should().BeTrue();
        AccessKey.Create(new string('1', 44)).Value.Value.Should().HaveLength(44);
    }
}
