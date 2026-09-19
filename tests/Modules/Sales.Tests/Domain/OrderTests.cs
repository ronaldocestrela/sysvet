using FluentAssertions;
using Sales.Domain.Entities;
using Sales.Domain.Enums;

namespace Sales.Tests.Domain;

public class OrderTests
{
    private static Order CreateDraftOrder()
    {
        var result = Order.Create(cashRegisterId: Guid.NewGuid());
        result.IsSuccess.Should().BeTrue();
        return result.Value;
    }

    [Fact]
    public void Create_WithPetWithoutTutor_ReturnsFailure()
    {
        var result = Order.Create(cashRegisterId: Guid.NewGuid(), tutorId: null, petId: Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.PetRequiresTutor");
    }

    [Fact]
    public void Create_WithTutorAndPet_ReturnsSuccess()
    {
        var tutorId = Guid.NewGuid();
        var petId = Guid.NewGuid();

        var result = Order.Create(cashRegisterId: Guid.NewGuid(), tutorId, petId);

        result.IsSuccess.Should().BeTrue();
        result.Value.TutorId.Should().Be(tutorId);
        result.Value.PetId.Should().Be(petId);
        result.Value.Status.Should().Be(OrderStatus.Draft);
    }

    [Fact]
    public void Create_WithClientId_UsesProvidedId()
    {
        var orderId = Guid.NewGuid();
        var cashRegisterId = Guid.NewGuid();

        var result = Order.Create(orderId, cashRegisterId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(orderId);
        result.Value.CashRegisterId.Should().Be(cashRegisterId);
    }

    [Fact]
    public void Create_WithEmptyClientId_ReturnsFailure()
    {
        var result = Order.Create(Guid.Empty, Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void AddItem_Product_RequiresProductId()
    {
        var order = CreateDraftOrder();

        var result = order.AddProductItem(Guid.Empty, "Ração", 1m, 10m);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void AddItem_Service_RequiresDescription()
    {
        var order = CreateDraftOrder();

        var result = order.AddServiceItem("  ", 1m, 50m);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void AddItem_Service_DoesNotRequireProductId()
    {
        var order = CreateDraftOrder();

        var result = order.AddServiceItem("Consulta", 1m, 80m);

        result.IsSuccess.Should().BeTrue();
        order.Items.Should().ContainSingle(i =>
            i.Kind == OrderItemKind.Service && i.ProductId == null && i.ProductName == "Consulta");
    }

    [Fact]
    public void Pay_WithEmptyItems_ReturnsFailure()
    {
        var order = CreateDraftOrder();
        var payments = new[] { Payment.Create(PaymentMethod.Cash, 10m).Value };

        var result = order.Pay(payments);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Pay_WithPaymentTotalMismatch_ReturnsFailure()
    {
        var order = CreateDraftOrder();
        order.AddProductItem(Guid.NewGuid(), "Item", 1m, 100m);

        var payments = new[] { Payment.Create(PaymentMethod.Cash, 50m).Value };

        var result = order.Pay(payments);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.PaymentTotalMismatch");
    }

    [Fact]
    public void Pay_WithValidSplit_SetsPaidAndFinancePending()
    {
        var order = CreateDraftOrder();
        order.AddProductItem(Guid.NewGuid(), "Prod", 2m, 25m);
        order.AddServiceItem("Banho", 1m, 50m);

        var payments = new[]
        {
            Payment.Create(PaymentMethod.Cash, 50m).Value,
            Payment.Create(PaymentMethod.Pix, 50m).Value
        };

        var result = order.Pay(payments);

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Paid);
        order.FinanceIntegrationStatus.Should().Be(FinanceIntegrationStatus.Pending);
        order.PaidAt.Should().NotBeNull();
        order.Payments.Should().HaveCount(2);
        order.TotalAmount.Amount.Should().Be(100m);
    }

    [Fact]
    public void AddItem_WhenNotDraft_ReturnsFailure()
    {
        var order = CreateDraftOrder();
        order.AddProductItem(Guid.NewGuid(), "P", 1m, 10m);
        order.Pay(new[] { Payment.Create(PaymentMethod.Cash, 10m).Value });

        var result = order.AddProductItem(Guid.NewGuid(), "X", 1m, 1m);

        result.IsFailure.Should().BeTrue();
    }
}

public class MoneyTests
{
    [Fact]
    public void Create_WithNegativeAmount_ReturnsFailure()
    {
        Sales.Domain.ValueObjects.Money.Create(-1m).IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Operators_AddAndSubtract_WorkForNonNegativeResult()
    {
        var left = Sales.Domain.ValueObjects.Money.CreateUnsafe(20m);
        var right = Sales.Domain.ValueObjects.Money.CreateUnsafe(5m);

        (left + right).Amount.Should().Be(25m);
        (left - right).Amount.Should().Be(15m);
        left.ToString().Should().Contain("BRL");
    }
}
