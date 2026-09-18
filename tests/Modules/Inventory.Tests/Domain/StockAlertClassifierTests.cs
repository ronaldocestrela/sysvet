using FluentAssertions;
using Inventory.Domain.Services;
using Xunit;

namespace Inventory.Tests.Domain;

public class StockAlertClassifierTests
{
    [Theory]
    [InlineData(5, 10, true)]
    [InlineData(10, 10, true)]
    [InlineData(11, 10, false)]
    [InlineData(5, 0, false)]
    public void IsLowStock_ReturnsExpected(decimal qty, decimal reorder, bool expected) =>
        StockAlertClassifier.IsLowStock(qty, reorder).Should().Be(expected);

    [Fact]
    public void ClassifyLotExpiry_Expired_ReturnsExpired()
    {
        var now = new DateTimeOffset(2026, 9, 18, 12, 0, 0, TimeSpan.Zero);
        var kind = StockAlertClassifier.ClassifyLotExpiry(now.AddDays(-1), now, 30);
        kind.Should().Be(StockAlertKind.Expired);
    }

    [Fact]
    public void ClassifyLotExpiry_WithinHorizon_ReturnsExpiringSoon()
    {
        var now = new DateTimeOffset(2026, 9, 18, 12, 0, 0, TimeSpan.Zero);
        var kind = StockAlertClassifier.ClassifyLotExpiry(now.AddDays(10), now, 30);
        kind.Should().Be(StockAlertKind.ExpiringSoon);
    }
}
