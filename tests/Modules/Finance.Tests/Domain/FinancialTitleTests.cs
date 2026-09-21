using Finance.Domain.Entities;
using Finance.Domain.Enums;
using Finance.Domain.Models;
using FluentAssertions;

namespace Finance.Tests.Domain;

public class FinancialTitleTests
{
    private static readonly Guid CategoryId = Guid.NewGuid();

    [Fact]
    public void CreateFromSale_WithPayments_ShouldBeSettled()
    {
        var orderId = Guid.NewGuid();
        var payments = new List<SalePaymentSlice>
        {
            new("Cash", 60m, null, Guid.NewGuid()),
            new("Pix", 40m, "NSU1", Guid.NewGuid())
        };

        var result = FinancialTitle.CreateFromSale(
            orderId,
            CategoryId,
            100m,
            Guid.NewGuid(),
            "Pedido PDV",
            payments,
            DateOnly.FromDateTime(DateTime.UtcNow));

        result.IsSuccess.Should().BeTrue();
        var title = result.Value;
        title.Direction.Should().Be(TitleDirection.Receivable);
        title.Status.Should().Be(TitleStatus.Settled);
        title.SourceType.Should().Be(TitleSourceType.Sale);
        title.SourceId.Should().Be(orderId);
        title.Allocations.Should().HaveCount(2);
    }

    [Fact]
    public void CreateFromPurchaseDuplicate_ShouldBeOpen()
    {
        var result = FinancialTitle.CreateFromPurchaseDuplicate(
            Guid.NewGuid(),
            CategoryId,
            Guid.NewGuid(),
            "001",
            250m,
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
            "NF-e compra");

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(TitleStatus.Open);
        result.Value.Direction.Should().Be(TitleDirection.Payable);
        result.Value.Allocations.Should().BeEmpty();
    }

    [Fact]
    public void Allocate_Partial_ShouldBePartiallySettled()
    {
        var title = FinancialTitle.CreateManual(
            TitleDirection.Payable,
            CategoryId,
            PartyKind.Supplier,
            Guid.NewGuid(),
            100m,
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
            "Manual AP").Value;

        var allocate = title.Allocate(40m, DateTimeOffset.UtcNow, "Cash", Guid.NewGuid());

        allocate.IsSuccess.Should().BeTrue();
        title.Status.Should().Be(TitleStatus.PartiallySettled);
        title.OpenAmount.Should().Be(60m);
    }

    [Fact]
    public void ReverseAllocation_AfterSale_ShouldReopenTitle()
    {
        var correlation = Guid.NewGuid();
        var title = FinancialTitle.CreateFromSale(
            Guid.NewGuid(),
            CategoryId,
            50m,
            null,
            "Venda",
            [new SalePaymentSlice("Cash", 50m, null, correlation)],
            DateOnly.FromDateTime(DateTime.UtcNow)).Value;

        title.Status.Should().Be(TitleStatus.Settled);

        var reverse = title.ReverseAllocation(20m, DateTimeOffset.UtcNow, "Cash", Guid.NewGuid());

        reverse.IsSuccess.Should().BeTrue();
        title.Status.Should().Be(TitleStatus.PartiallySettled);
        title.OpenAmount.Should().Be(20m);
    }

    [Fact]
    public void Cancel_ManualOpen_ShouldSucceed()
    {
        var title = FinancialTitle.CreateManual(
            TitleDirection.Receivable,
            CategoryId,
            PartyKind.Tutor,
            Guid.NewGuid(),
            80m,
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow),
            "Fiado manual").Value;

        title.Cancel().IsSuccess.Should().BeTrue();
        title.Status.Should().Be(TitleStatus.Cancelled);
    }
}
