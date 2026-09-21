namespace Finance.Domain.Enums;

/// <summary>Result of matching an acquirer statement line to a receivable allocation.</summary>
public enum CardReconciliationLineStatus
{
    Unmatched = 0,
    Matched = 1,
    Divergent = 2
}
