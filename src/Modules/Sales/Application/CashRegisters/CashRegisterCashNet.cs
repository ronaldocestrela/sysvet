using Sales.Domain.Enums;
using Sales.Domain.Queries;

namespace Sales.Application.CashRegisters;

internal static class CashRegisterCashNet
{
    public static decimal FromTotals(IReadOnlyList<CashRegisterPaymentTotals> totals)
    {
        var cash = totals.FirstOrDefault(t => t.Method == PaymentMethod.Cash);
        return cash is null ? 0m : cash.Gross - cash.Refunded;
    }
}
