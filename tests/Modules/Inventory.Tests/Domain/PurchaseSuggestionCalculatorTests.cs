using FluentAssertions;
using Inventory.Domain.Services;
using Xunit;

namespace Inventory.Tests.Domain;

public class PurchaseSuggestionCalculatorTests
{
    [Fact]
    public void ComputeSuggestedQuantity_WhenLowStock_UsesTargetStockMinusOnHand()
    {
        var qty = PurchaseSuggestionCalculator.ComputeSuggestedQuantity(
            onHand: 3m,
            reorderLevel: 5m,
            targetStock: 20m,
            unitsPerPackage: 1m);

        qty.Should().Be(17m);
    }

    [Fact]
    public void ComputeSuggestedQuantity_WhenTargetStockZero_FallsBackToReorderLevel()
    {
        var qty = PurchaseSuggestionCalculator.ComputeSuggestedQuantity(
            onHand: 2m,
            reorderLevel: 10m,
            targetStock: 0m,
            unitsPerPackage: 1m);

        qty.Should().Be(8m);
    }

    [Fact]
    public void ComputeSuggestedQuantity_RoundsUpToPackageUnits()
    {
        var qty = PurchaseSuggestionCalculator.ComputeSuggestedQuantity(
            onHand: 0m,
            reorderLevel: 5m,
            targetStock: 10m,
            unitsPerPackage: 6m);

        qty.Should().Be(12m);
    }

    [Fact]
    public void ComputeSuggestedQuantity_WhenNotLowStock_ReturnsNull()
    {
        var qty = PurchaseSuggestionCalculator.ComputeSuggestedQuantity(
            onHand: 20m,
            reorderLevel: 5m,
            targetStock: 10m,
            unitsPerPackage: 1m);

        qty.Should().BeNull();
    }

    [Fact]
    public void ComputeSuggestedQuantity_WhenAlreadyAtTarget_ReturnsNull()
    {
        var qty = PurchaseSuggestionCalculator.ComputeSuggestedQuantity(
            onHand: 10m,
            reorderLevel: 5m,
            targetStock: 10m,
            unitsPerPackage: 1m);

        qty.Should().BeNull();
    }

    [Fact]
    public void EffectiveTargetStock_PrefersTargetWhenPositive()
    {
        PurchaseSuggestionCalculator.EffectiveTargetStock(5m, 20m).Should().Be(20m);
        PurchaseSuggestionCalculator.EffectiveTargetStock(5m, 0m).Should().Be(5m);
    }
}
