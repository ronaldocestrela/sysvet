namespace Platform.Application.Abstractions;

/// <summary>Observes SaaS billing charge failures for operational alerting (10.5).</summary>
public interface IBillingChargeObserver
{
    /// <summary>Called when a gateway charge fails and the invoice is marked failed.</summary>
    void OnChargeFailure(Guid tenantId, Guid invoiceId);
}
