namespace Sales.Domain.Enums;

/// <summary>Accounts receivable linkage until Finance module consumes the sale.</summary>
public enum FinanceIntegrationStatus
{
    None = 0,
    Pending = 1,
    Linked = 2
}
