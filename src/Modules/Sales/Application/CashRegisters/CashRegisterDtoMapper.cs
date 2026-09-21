using Sales.Application.CashRegisters.Dtos;
using Sales.Domain.Entities;
using Sales.Domain.Queries;

namespace Sales.Application.CashRegisters;

internal static class CashRegisterDtoMapper
{
    public static IReadOnlyList<CashRegisterMethodTotalsDto> MapTotals(IReadOnlyList<CashRegisterPaymentTotals> totals) =>
        totals.Select(t => new CashRegisterMethodTotalsDto
        {
            Method = t.Method,
            Gross = t.Gross,
            Refunded = t.Refunded
        }).ToList();

    public static IReadOnlyList<CashMovementDto> MapMovements(CashRegister register) =>
        register.Movements.Select(m => new CashMovementDto
        {
            Id = m.Id,
            Kind = m.Kind,
            Amount = m.Amount.Amount,
            Reason = m.Reason,
            OccurredAt = m.OccurredAt
        }).ToList();

    public static CashRegisterDetailDto ToDetail(CashRegister register, IReadOnlyList<CashRegisterPaymentTotals> totals)
    {
        var cashNet = CashRegisterCashNet.FromTotals(totals);
        var expected = register.Status == Domain.Enums.CashRegisterStatus.Open
            ? register.ComputeExpectedCash(cashNet)
            : register.ExpectedClosingBalance.Amount;

        return new CashRegisterDetailDto
        {
            Id = register.Id,
            Status = register.Status.ToString(),
            OpeningBalance = register.OpeningBalance.Amount,
            ExpectedClosingBalance = expected,
            ClosingBalance = register.ClosingBalance.Amount,
            ClosingVariance = register.ClosingVariance,
            OpenedAt = register.OpenedAt,
            ClosedAt = register.ClosedAt,
            MethodTotals = MapTotals(totals),
            Movements = MapMovements(register)
        };
    }
}
