using Core.Domain;
using Platform.Domain.Entities;
using Platform.Domain.Repositories;

namespace Platform.Application.Billing;

/// <summary>Shared settlement steps after invoice payment (9.4).</summary>
public static class BillingInvoiceSettlement
{
    /// <summary>Marks invoice paid, settles adjustments and advances subscription period.</summary>
    public static async Task<Result> ApplyPaidAsync(
        BillingInvoice invoice,
        TenantSubscription subscription,
        ISubscriptionAdjustmentRepository adjustmentRepository,
        DateTimeOffset paidAtUtc,
        CancellationToken cancellationToken)
    {
        var markPaid = invoice.MarkPaid(paidAtUtc);
        if (markPaid.IsFailure)
        {
            return markPaid;
        }

        var adjustments = await adjustmentRepository.ListByInvoiceIdAsync(invoice.Id, cancellationToken);
        foreach (var adjustment in adjustments)
        {
            var settled = adjustment.MarkSettled();
            if (settled.IsFailure)
            {
                return settled;
            }
        }

        return subscription.RecordPaymentSuccess(paidAtUtc);
    }
}
