namespace Finance.Domain.Models;

/// <summary>
/// One line from an imported acquirer statement batch.
/// </summary>
public sealed record CardStatementImportLine(
    string Nsu,
    decimal Amount,
    string? Method,
    decimal? Fee,
    DateTimeOffset OccurredAt);
