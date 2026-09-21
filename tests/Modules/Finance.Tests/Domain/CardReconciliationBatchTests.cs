using Finance.Domain.Entities;
using Finance.Domain.Enums;
using Finance.Domain.Models;
using FluentAssertions;

namespace Finance.Tests.Domain;

public class CardReconciliationBatchTests
{
    [Fact]
    public void Import_WhenNsuAndAmountMatch_SetsMatched()
    {
        var allocationId = Guid.NewGuid();
        var settlements = new List<CardSettlementSnapshot>
        {
            new(allocationId, "NSU123", 100m, "CreditCard")
        };

        var result = CardReconciliationBatch.Import(
            "Lote 1",
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow),
            [new CardStatementImportLine("NSU123", 100m, "CreditCard", null, DateTimeOffset.UtcNow)],
            settlements);

        result.IsSuccess.Should().BeTrue();
        result.Value.Lines.Single().Status.Should().Be(CardReconciliationLineStatus.Matched);
        result.Value.Lines.Single().MatchedAllocationId.Should().Be(allocationId);
    }

    [Fact]
    public void Import_WhenNsuExistsButAmountDiffers_SetsDivergent()
    {
        var settlements = new List<CardSettlementSnapshot>
        {
            new(Guid.NewGuid(), "NSU123", 100m, "CreditCard")
        };

        var result = CardReconciliationBatch.Import(
            "Lote 1",
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow),
            [new CardStatementImportLine("NSU123", 99m, "CreditCard", null, DateTimeOffset.UtcNow)],
            settlements);

        result.IsSuccess.Should().BeTrue();
        result.Value.Lines.Single().Status.Should().Be(CardReconciliationLineStatus.Divergent);
    }

    [Fact]
    public void Import_WhenNsuMissing_SetsUnmatched()
    {
        var result = CardReconciliationBatch.Import(
            "Lote 1",
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow),
            [new CardStatementImportLine("UNKNOWN", 50m, "DebitCard", null, DateTimeOffset.UtcNow)],
            Array.Empty<CardSettlementSnapshot>());

        result.IsSuccess.Should().BeTrue();
        result.Value.Lines.Single().Status.Should().Be(CardReconciliationLineStatus.Unmatched);
    }
}
