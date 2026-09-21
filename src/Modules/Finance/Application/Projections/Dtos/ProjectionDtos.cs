namespace Finance.Application.Projections.Dtos;

/// <summary>Expected vs realized balances for a period.</summary>
public sealed class BalanceProjectionDto
{
    public DateOnly From { get; init; }
    public DateOnly To { get; init; }
    public decimal ExpectedReceivable { get; init; }
    public decimal ExpectedPayable { get; init; }
    public decimal RealizedReceivable { get; init; }
    public decimal RealizedPayable { get; init; }
}

/// <summary>Ledger entry for a party.</summary>
public sealed class PartyLedgerEntryDto
{
    public Guid TitleId { get; init; }
    public string Description { get; init; } = string.Empty;
    public DateOnly DueDate { get; init; }
    public decimal OriginalAmount { get; init; }
    public decimal OpenAmount { get; init; }
    public string Status { get; init; } = string.Empty;
}
