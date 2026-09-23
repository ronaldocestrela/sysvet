namespace Platform.Domain.Entities;

/// <summary>Lifecycle of a platform billing invoice (9.4).</summary>
public enum BillingInvoiceStatus
{
    /// <summary>Issued; awaiting gateway confirmation.</summary>
    Open = 1,

    /// <summary>Liquidated via webhook or zero-amount local settlement.</summary>
    Paid = 2,

    /// <summary>Overdue or failed collection.</summary>
    Failed = 3,

    /// <summary>Canceled before payment.</summary>
    Canceled = 4,

    /// <summary>Payment refunded after settlement.</summary>
    Refunded = 5
}
