using Sales.Domain.Enums;

namespace Sales.Application.CashRegisters.Dtos;

/// <summary>Open cash register session for the current operator.</summary>
public sealed class OpenCashRegisterDto
{
    public Guid Id { get; init; }
    public string Status { get; init; } = string.Empty;
    public decimal OpeningBalance { get; init; }
    public decimal CurrentBalance { get; init; }
    public IReadOnlyList<CashRegisterMethodTotalsDto> MethodTotals { get; init; } =
        Array.Empty<CashRegisterMethodTotalsDto>();
}

/// <summary>Gross, refunded, and net amounts per payment method in the session.</summary>
public sealed class CashRegisterMethodTotalsDto
{
    public PaymentMethod Method { get; init; }
    public decimal Gross { get; init; }
    public decimal Refunded { get; init; }
    public decimal Net => Gross - Refunded;
}
