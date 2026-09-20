using FluentAssertions;
using Sales.Domain.Entities;

namespace Sales.Tests.Domain;

public class ProductKitTests
{
    [Fact]
    public void Create_WithNoComponents_ReturnsFailure()
    {
        var result = ProductKit.Create("Kit vazio", Array.Empty<(Guid, decimal)>());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Kit.EmptyComponents");
    }

    [Fact]
    public void ExplodeStockLines_MultipliesByKitQuantity()
    {
        var p1 = Guid.NewGuid();
        var p2 = Guid.NewGuid();
        var kit = ProductKit.Create("Combo", new[] { (p1, 2m), (p2, 1m) }).Value;

        var lines = kit.ExplodeStockLines(3m);

        lines.Should().Contain((p1, 6m));
        lines.Should().Contain((p2, 3m));
    }
}
