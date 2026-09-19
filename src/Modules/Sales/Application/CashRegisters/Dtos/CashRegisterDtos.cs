namespace Sales.Application.CashRegisters.Dtos;

/// <summary>Open cash register session for the current operator.</summary>
public sealed class OpenCashRegisterDto
{
    public Guid Id { get; init; }
    public string Status { get; init; } = string.Empty;
    public decimal OpeningBalance { get; init; }
    public decimal CurrentBalance { get; init; }
}
