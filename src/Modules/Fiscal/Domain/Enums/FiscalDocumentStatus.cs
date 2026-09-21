namespace Fiscal.Domain.Enums;

/// <summary>Lifecycle of an outbound fiscal document.</summary>
public enum FiscalDocumentStatus
{
    Draft = 0,
    Transmitting = 1,
    Authorized = 2,
    Rejected = 3,
    Denied = 4,
    Cancelled = 5
}
