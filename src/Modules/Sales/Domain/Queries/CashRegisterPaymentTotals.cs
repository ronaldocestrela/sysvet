using Sales.Domain.Enums;

namespace Sales.Domain.Queries;

/// <summary>Aggregated payment amounts for a cash register session query.</summary>
public sealed class CashRegisterPaymentTotals
{
    public PaymentMethod Method { get; init; }
    public decimal Gross { get; init; }
    public decimal Refunded { get; init; }
}
