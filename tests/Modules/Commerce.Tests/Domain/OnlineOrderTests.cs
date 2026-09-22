using Commerce.Domain;
using Commerce.Domain.Entities;
using Commerce.Domain.Enums;
using Commerce.Domain.ValueObjects;
using FluentAssertions;

namespace Commerce.Tests.Domain;

public class OnlineOrderTests
{
    [Fact]
    public void Confirm_WithLines_ShouldSetConfirmedStatus()
    {
        var price = Money.Create(10m).Value;
        var lines = new List<(Guid, Guid, string, string, decimal, Money)>
        {
            (Guid.NewGuid(), Guid.NewGuid(), "Produto", "SKU", 1m, price)
        };

        var order = OnlineOrder.CreateStorePickup("Maria", "11999998888", "a@b.com", lines).Value;
        var confirm = order.Confirm();

        confirm.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OnlineOrderStatus.Confirmed);
        order.TotalAmount.Should().Be(10m);
    }

    [Fact]
    public void Confirm_WithoutBuyerPhone_ShouldFailOnCreate()
    {
        var price = Money.Create(10m).Value;
        var lines = new List<(Guid, Guid, string, string, decimal, Money)>
        {
            (Guid.NewGuid(), Guid.NewGuid(), "Produto", "SKU", 1m, price)
        };

        var order = OnlineOrder.CreateStorePickup("Maria", "", null, lines);

        order.IsFailure.Should().BeTrue();
        order.Error.Code.Should().Be(ErrorCodes.Order.BuyerRequired.Code);
    }

    [Fact]
    public void Cancel_AfterConfirm_ShouldRequireStockRestore()
    {
        var price = Money.Create(5m).Value;
        var lines = new List<(Guid, Guid, string, string, decimal, Money)>
        {
            (Guid.NewGuid(), Guid.NewGuid(), "Produto", "SKU", 2m, price)
        };

        var order = OnlineOrder.CreateStorePickup("João", "11988887777", null, lines).Value;
        order.Confirm();
        order.Cancel();

        order.Status.Should().Be(OnlineOrderStatus.Cancelled);
        order.RequiresStockRestore().Should().BeTrue();
    }
}
