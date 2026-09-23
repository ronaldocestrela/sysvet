namespace Platform.Domain.Entities;

/// <summary>Billing state of a subscription proration row (charged in 9.4).</summary>
public enum AdjustmentStatus
{
    /// <summary>Awaiting payment gateway integration.</summary>
    PendingBilling = 1,

    /// <summary>Included on an open or paid invoice.</summary>
    Invoiced = 2,

    /// <summary>Invoice paid; adjustment fully applied.</summary>
    Settled = 3
}
