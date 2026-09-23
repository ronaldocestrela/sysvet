using FluentAssertions;
using Platform.Domain.Entities;
using Platform.Domain.Services;

namespace Platform.Tests.Domain;

public class BillingInvoiceComposerTests
{
    [Fact]
    public void ComposeAmount_SumsPlanAddOnAndPendingAdjustment()
    {
        var tenantId = Guid.NewGuid();
        var adjustment = SubscriptionAdjustment.CreatePending(tenantId, 50m, null, Guid.NewGuid()).Value;

        var total = BillingInvoiceComposer.ComposeAmount(
            199m,
            new[] { 79m },
            new[] { adjustment });

        total.Should().Be(328m);
    }

    [Fact]
    public void ComposeAmount_IgnoresNegativePendingAdjustments()
    {
        var tenantId = Guid.NewGuid();
        var credit = SubscriptionAdjustment.CreatePending(tenantId, -20m, Guid.NewGuid(), null).Value;

        var total = BillingInvoiceComposer.ComposeAmount(199m, Array.Empty<decimal>(), new[] { credit });

        total.Should().Be(199m);
    }
}
