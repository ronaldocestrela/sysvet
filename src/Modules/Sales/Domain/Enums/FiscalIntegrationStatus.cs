namespace Sales.Domain.Enums;

/// <summary>Fiscal document linkage for paid orders.</summary>
public enum FiscalIntegrationStatus
{
    None = 0,
    Pending = 1,
    Linked = 2,
    Partial = 3
}
