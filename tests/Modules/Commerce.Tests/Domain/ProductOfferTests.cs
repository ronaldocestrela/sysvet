using Commerce.Domain;
using Commerce.Domain.Entities;
using FluentAssertions;

namespace Commerce.Tests.Domain;

public class ProductOfferTests
{
    [Fact]
    public void Create_WithZeroPrice_ShouldFail()
    {
        var result = ProductOffer.Create(Guid.NewGuid(), "SKU-1", "Ração", 0m);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(ErrorCodes.Offer.InvalidPrice.Code);
    }

    [Fact]
    public void UpdateCatalogSnapshot_ShouldChangeSalePrice()
    {
        var offer = ProductOffer.Create(Guid.NewGuid(), "SKU-1", "Ração", 29.9m).Value;

        var update = offer.UpdateCatalogSnapshot("SKU-1", "Ração Premium", 34.5m);

        update.IsSuccess.Should().BeTrue();
        offer.SalePrice.Amount.Should().Be(34.5m);
        offer.ProductName.Should().Be("Ração Premium");
    }
}
