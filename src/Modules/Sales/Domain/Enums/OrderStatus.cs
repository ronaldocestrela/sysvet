namespace Sales.Domain.Enums;

/// <summary>Lifecycle of a sales order at the POS.</summary>
public enum OrderStatus
{
    Draft = 0,
    PendingPayment = 1,
    Paid = 2,
    PartiallyRefunded = 3,
    Refunded = 4
}
