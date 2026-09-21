using Finance.Domain.Entities;
using Finance.Domain.Enums;
using Finance.Domain.Models;
using Finance.Domain.Services;
using FluentAssertions;

namespace Finance.Tests.Domain;

public class FinancialStatementCalculatorTests
{
    private static readonly Guid SalesCategoryId = Guid.NewGuid();
    private static readonly Guid PurchasesCategoryId = Guid.NewGuid();

    private static IReadOnlyDictionary<Guid, FinancialCategory> Categories =>
        new Dictionary<Guid, FinancialCategory>
        {
            [SalesCategoryId] = FinancialCategory.CreateSystem("SALES", "Vendas", CategoryDirection.In),
            [PurchasesCategoryId] = FinancialCategory.CreateSystem("PURCHASES", "Compras", CategoryDirection.Out)
        };

    [Fact]
    public void BuildCashFlow_InvalidRange_ShouldFail()
    {
        var result = FinancialStatementCalculator.BuildCashFlow(
            new DateOnly(2026, 9, 10),
            new DateOnly(2026, 9, 1),
            Array.Empty<FinancialTitle>());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Finance.Report.InvalidDateRange");
    }

    [Fact]
    public void BuildCashFlow_PaymentOutsideDueWindow_ShouldCountRealizedOnPaidDate()
    {
        var paidAt = new DateTimeOffset(2026, 9, 15, 12, 0, 0, TimeSpan.Zero);
        var title = FinancialTitle.CreateManual(
            TitleDirection.Payable,
            PurchasesCategoryId,
            PartyKind.Supplier,
            Guid.NewGuid(),
            80m,
            new DateOnly(2026, 10, 1),
            new DateOnly(2026, 10, 31),
            "AP late due").Value;

        title.Allocate(80m, paidAt, "Pix", Guid.NewGuid()).IsSuccess.Should().BeTrue();

        var from = new DateOnly(2026, 9, 1);
        var to = new DateOnly(2026, 9, 30);
        var result = FinancialStatementCalculator.BuildCashFlow(from, to, new[] { title });

        result.IsSuccess.Should().BeTrue();
        var day = result.Value.Days.Single(d => d.Date == new DateOnly(2026, 9, 15));
        day.RealizedOutflow.Should().Be(80m);
        result.Value.TotalRealizedOutflow.Should().Be(80m);
    }

    [Fact]
    public void BuildCashFlow_Reversal_ShouldNetRealizedInflows()
    {
        var orderId = Guid.NewGuid();
        var payDay = new DateOnly(2026, 9, 5);
        var payments = new List<SalePaymentSlice> { new("Cash", 100m, null, Guid.NewGuid()) };
        var title = FinancialTitle.CreateFromSale(
            orderId,
            SalesCategoryId,
            100m,
            Guid.NewGuid(),
            "Sale",
            payments,
            payDay).Value;

        var reversalDay = new DateTimeOffset(2026, 9, 20, 10, 0, 0, TimeSpan.Zero);
        title.ReverseAllocation(30m, reversalDay, "Cash", Guid.NewGuid()).IsSuccess.Should().BeTrue();

        var result = FinancialStatementCalculator.BuildCashFlow(
            new DateOnly(2026, 9, 1),
            new DateOnly(2026, 9, 30),
            new[] { title });

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalRealizedInflow.Should().Be(70m);
    }

    [Fact]
    public void BuildSimplifiedDre_CancelledTitle_ShouldExclude()
    {
        var issue = new DateOnly(2026, 9, 10);
        var title = FinancialTitle.CreateManual(
            TitleDirection.Receivable,
            SalesCategoryId,
            PartyKind.Tutor,
            Guid.NewGuid(),
            200m,
            issue,
            issue.AddDays(15),
            "Cancelled AR").Value;

        title.Cancel().IsSuccess.Should().BeTrue();

        var result = FinancialStatementCalculator.BuildSimplifiedDre(2026, 9, new[] { title }, Categories);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalRevenue.Should().Be(0m);
        result.Value.Lines.Should().BeEmpty();
    }

    [Fact]
    public void BuildSimplifiedDre_ReversalInMonth_ShouldReduceCategoryRevenue()
    {
        var issue = new DateOnly(2026, 9, 1);
        var payments = new List<SalePaymentSlice> { new("Cash", 100m, null, Guid.NewGuid()) };
        var title = FinancialTitle.CreateFromSale(
            Guid.NewGuid(),
            SalesCategoryId,
            100m,
            Guid.NewGuid(),
            "Sale",
            payments,
            issue).Value;

        title.ReverseAllocation(
            25m,
            new DateTimeOffset(2026, 9, 18, 0, 0, 0, TimeSpan.Zero),
            "Cash",
            Guid.NewGuid()).IsSuccess.Should().BeTrue();

        var result = FinancialStatementCalculator.BuildSimplifiedDre(2026, 9, new[] { title }, Categories);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalRevenue.Should().Be(75m);
        result.Value.Lines.Should().ContainSingle(l => l.CategoryCode == "SALES" && l.Revenue == 75m);
    }

    [Fact]
    public void BuildSimplifiedDre_InvalidMonth_ShouldFail()
    {
        var result = FinancialStatementCalculator.BuildSimplifiedDre(2026, 13, Array.Empty<FinancialTitle>(), Categories);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Finance.Report.InvalidMonth");
    }
}
