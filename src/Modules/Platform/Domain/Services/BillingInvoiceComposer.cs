using Platform.Domain.Entities;

namespace Platform.Domain.Services;

/// <summary>Calculates recurring invoice totals from catalog and pending adjustments (9.4).</summary>
public static class BillingInvoiceComposer
{
    /// <summary>Composes invoice amount from plan, active add-ons and positive pending adjustments.</summary>
    public static decimal ComposeAmount(
        decimal planMonthlyPrice,
        IEnumerable<decimal> activeAddOnPrices,
        IEnumerable<SubscriptionAdjustment> pendingAdjustments)
    {
        var adjustmentTotal = pendingAdjustments
            .Where(a => a.Status == AdjustmentStatus.PendingBilling && a.Amount > 0)
            .Sum(a => a.Amount);

        return planMonthlyPrice + activeAddOnPrices.Sum() + adjustmentTotal;
    }
}
