using FluentAssertions;
using Sales.Domain.Entities;
using Sales.Domain.Enums;
using Sales.Domain.Services;

namespace Sales.Tests.Domain;

public class CommissionCalculatorTests
{
    private static readonly Guid SellerId = Guid.NewGuid();
    private static readonly Guid VetId = Guid.NewGuid();

    [Fact]
    public void Calculate_WithSellerRule_AccruesOnProductAndService()
    {
        var order = PaidOrder();
        order.AddProductItem(Guid.NewGuid(), "Ração", 1m, 100m);
        order.AddServiceItem("Consulta", 1m, 50m, VetId, CommissionRole.Veterinarian);
        order.Pay([Payment.Create(PaymentMethod.Cash, 150m).Value]);

        var rules = new List<CommissionRule>
        {
            CommissionRule.Create(CommissionRole.Seller, CommissionAppliesTo.All, 10m).Value,
            CommissionRule.Create(CommissionRole.Veterinarian, CommissionAppliesTo.Service, 20m).Value
        };

        var accruals = CommissionCalculator.Calculate(order, SellerId, rules);

        accruals.Should().HaveCount(3);
        accruals.Single(a => a.Role == CommissionRole.Seller && a.BaseAmount.Amount == 100m).CommissionAmount.Amount.Should().Be(10m);
        accruals.Single(a => a.Role == CommissionRole.Seller && a.BaseAmount.Amount == 50m).CommissionAmount.Amount.Should().Be(5m);
        accruals.Single(a => a.Role == CommissionRole.Veterinarian).CommissionAmount.Amount.Should().Be(10m);
    }

    [Fact]
    public void Calculate_WithOrderDiscount_UsesNetLineBase()
    {
        var order = PaidOrder();
        order.ApplyDiscount(10m);
        order.AddProductItem(Guid.NewGuid(), "P", 1m, 100m);
        order.Pay([Payment.Create(PaymentMethod.Cash, 90m).Value]);

        var rules = new List<CommissionRule>
        {
            CommissionRule.Create(CommissionRole.Seller, CommissionAppliesTo.Product, 10m).Value
        };

        var accruals = CommissionCalculator.Calculate(order, SellerId, rules);

        accruals.Single().BaseAmount.Amount.Should().Be(90m);
        accruals.Single().CommissionAmount.Amount.Should().Be(9m);
    }

    private static Order PaidOrder()
    {
        var registerId = Guid.NewGuid();
        return Order.Create(registerId, SellerId).Value;
    }
}
