using Sales.Domain.Enums;

namespace Sales.Application.CashRegisters.Dtos;

/// <summary>Open cash register session for the current operator.</summary>
public sealed class OpenCashRegisterDto
{
    public Guid Id { get; init; }
    public string Status { get; init; } = string.Empty;
    public decimal OpeningBalance { get; init; }
    public decimal ExpectedBalance { get; init; }
    public decimal CurrentBalance { get; init; }
    public IReadOnlyList<CashRegisterMethodTotalsDto> MethodTotals { get; init; } =
        Array.Empty<CashRegisterMethodTotalsDto>();
    public IReadOnlyList<CashMovementDto> Movements { get; init; } = Array.Empty<CashMovementDto>();
}

/// <summary>Cash register session detail (open or closed).</summary>
public sealed class CashRegisterDetailDto
{
    public Guid Id { get; init; }
    public string Status { get; init; } = string.Empty;
    public decimal OpeningBalance { get; init; }
    public decimal ExpectedClosingBalance { get; init; }
    public decimal ClosingBalance { get; init; }
    public decimal? ClosingVariance { get; init; }
    public DateTimeOffset OpenedAt { get; init; }
    public DateTimeOffset? ClosedAt { get; init; }
    public IReadOnlyList<CashRegisterMethodTotalsDto> MethodTotals { get; init; } =
        Array.Empty<CashRegisterMethodTotalsDto>();
    public IReadOnlyList<CashMovementDto> Movements { get; init; } = Array.Empty<CashMovementDto>();
}

/// <summary>Gross, refunded, and net amounts per payment method in the session.</summary>
public sealed class CashRegisterMethodTotalsDto
{
    public PaymentMethod Method { get; init; }
    public decimal Gross { get; init; }
    public decimal Refunded { get; init; }
    public decimal Net => Gross - Refunded;
}

/// <summary>Sangria or suprimento on the session.</summary>
public sealed class CashMovementDto
{
    public Guid Id { get; init; }
    public CashMovementKind Kind { get; init; }
    public decimal Amount { get; init; }
    public string Reason { get; init; } = string.Empty;
    public DateTimeOffset OccurredAt { get; init; }
}
