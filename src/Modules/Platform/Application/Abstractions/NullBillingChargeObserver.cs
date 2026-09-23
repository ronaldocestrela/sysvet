namespace Platform.Application.Abstractions;

/// <summary>No-op billing charge observer.</summary>
public sealed class NullBillingChargeObserver : IBillingChargeObserver
{
    /// <inheritdoc />
    public void OnChargeFailure(Guid tenantId, Guid invoiceId)
    {
    }
}
