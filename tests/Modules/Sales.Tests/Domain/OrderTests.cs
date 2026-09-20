using FluentAssertions;
using Sales.Domain.Entities;
using Sales.Domain.Enums;

namespace Sales.Tests.Domain;

public class OrderTests
{
    private static readonly Guid DefaultSellerId = Guid.NewGuid();

    private static Order CreateDraftOrder()
    {
        var result = Order.Create(cashRegisterId: Guid.NewGuid(), sellerUserId: DefaultSellerId);
        result.IsSuccess.Should().BeTrue();
        return result.Value;
    }

    [Fact]
    public void Create_WithPetWithoutTutor_ReturnsFailure()
    {
        var result = Order.Create(cashRegisterId: Guid.NewGuid(), sellerUserId: DefaultSellerId, tutorId: null, petId: Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.PetRequiresTutor");
    }

    [Fact]
    public void Create_WithTutorAndPet_ReturnsSuccess()
    {
        var tutorId = Guid.NewGuid();
        var petId = Guid.NewGuid();

        var result = Order.Create(cashRegisterId: Guid.NewGuid(), sellerUserId: DefaultSellerId, tutorId, petId);

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

        var result = Order.Create(orderId, cashRegisterId, DefaultSellerId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(orderId);
        result.Value.CashRegisterId.Should().Be(cashRegisterId);
    }

    [Fact]
    public void Create_WithEmptyClientId_ReturnsFailure()
    {
        var result = Order.Create(Guid.Empty, Guid.NewGuid(), DefaultSellerId);

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
    public void AddPackageItem_WithTutorAndPet_Succeeds()
    {
        var tutorId = Guid.NewGuid();
        var petId = Guid.NewGuid();
        var order = Order.Create(
            cashRegisterId: Guid.NewGuid(),
            sellerUserId: DefaultSellerId,
            tutorId: tutorId,
            petId: petId).Value;

        var result = order.AddPackageItem(Guid.NewGuid(), "Pacote Banho", 1m, 100m);

        result.IsSuccess.Should().BeTrue();
        order.Items.Should().ContainSingle(i => i.Kind == OrderItemKind.Package);
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
            Payment.Create(PaymentMethod.Pix, 50m, nsu: "000000000099", provider: "Simulator").Value
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

    [Fact]
    public void RefundPayment_Cash_Partial_UpdatesStatus()
    {
        var order = CreateDraftOrder();
        order.AddProductItem(Guid.NewGuid(), "P", 1m, 100m);
        order.Pay(new[] { Payment.Create(PaymentMethod.Cash, 100m).Value });

        var paymentId = order.Payments.Single().Id;
        var refund = order.RefundPayment(paymentId, 30m);

        refund.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.PartiallyRefunded);
        order.Payments.Single().RemainingRefundable.Should().Be(70m);
    }

    [Fact]
    public void RefundPayment_Full_SetsRefunded()
    {
        var order = CreateDraftOrder();
        order.AddProductItem(Guid.NewGuid(), "P", 1m, 50m);
        order.Pay(new[]
        {
            Payment.Create(PaymentMethod.DebitCard, 50m, nsu: "222222222222", provider: "Simulator").Value
        });

        var paymentId = order.Payments.Single().Id;
        var refund = order.RefundPayment(paymentId, 50m, refundNsu: "333333333333");

        refund.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Refunded);
    }

    [Fact]
    public void RefundPayment_ExceedsRemaining_ReturnsFailure()
    {
        var order = CreateDraftOrder();
        order.AddProductItem(Guid.NewGuid(), "P", 1m, 50m);
        order.Pay(new[] { Payment.Create(PaymentMethod.Cash, 50m).Value });

        var paymentId = order.Payments.Single().Id;
        var result = order.RefundPayment(paymentId, 51m);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Payment.RefundExceedsRemaining");
    }

    [Fact]
    public void RefundPayment_WhenDraft_ReturnsFailure()
    {
        var order = CreateDraftOrder();
        order.AddProductItem(Guid.NewGuid(), "P", 1m, 10m);

        var result = order.RefundPayment(Guid.NewGuid(), 10m);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Payment.RefundNotAllowed");
    }

    [Fact]
    public void ApplyDiscount_ReducesTotalForPay()
    {
        var order = CreateDraftOrder();
        order.AddProductItem(Guid.NewGuid(), "P", 1m, 100m);
        order.ApplyDiscount(10m).IsSuccess.Should().BeTrue();

        order.TotalAmount.Amount.Should().Be(90m);
        order.Pay([Payment.Create(PaymentMethod.Cash, 90m).Value]).IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ApplyDiscount_WhenOver100_ReturnsFailure()
    {
        var order = CreateDraftOrder();
        var result = order.ApplyDiscount(101m);
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ReturnItems_Partial_ReversesCommissionAndSetsPartiallyReturned()
    {
        var order = CreateDraftOrder();
        order.AddProductItem(Guid.NewGuid(), "P", 2m, 50m);
        order.Pay([Payment.Create(PaymentMethod.Cash, 100m).Value]);

        var itemId = order.Items.Single().Id;
        order.AttachCommissions([
            CommissionAccrual.Restore(Guid.NewGuid(), order.Id, itemId, DefaultSellerId, CommissionRole.Seller, 10m, 100m, 10m, CommissionAccrualStatus.Accrued)
        ]);

        var result = order.ReturnItems(Guid.NewGuid(), [(itemId, 1m)]);

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.PartiallyReturned);
        result.Value.RefundAmount.Amount.Should().Be(50m);
        order.Commissions.Single().CommissionAmount.Amount.Should().Be(5m);
    }

    [Fact]
    public void ReturnItems_WhenDraft_ReturnsFailure()
    {
        var order = CreateDraftOrder();
        order.AddProductItem(Guid.NewGuid(), "P", 1m, 10m);

        var result = order.ReturnItems(Guid.NewGuid(), [(order.Items.Single().Id, 1m)]);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.ReturnNotAllowed");
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
