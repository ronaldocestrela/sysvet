using Platform.Domain.Entities;

namespace Platform.Application.Metrics;

/// <summary>Delinquent tenant line for Super Admin UI (10.2).</summary>
public sealed record SaasDelinquencyItemDto(
    Guid TenantId,
    string DisplayName,
    decimal OutstandingAmount,
    DateTimeOffset? PastDueSince,
    BillingStanding BillingStanding);

/// <summary>Global SaaS metrics snapshot (10.2).</summary>
public sealed record PlatformSaasMetricsDto(
    int Year,
    int Month,
    decimal BilledMrr,
    decimal ContractedMrr,
    decimal Arr,
    decimal CashIn,
    decimal CashOut,
    decimal NetCashFlow,
    decimal LogoChurnRate,
    int PayingTenantsInMonth,
    int CancelledLogosInMonth,
    int PayingLogosAtMonthStart,
    decimal? Ltv,
    decimal AcquisitionSpend,
    int NewPayingTenantsInMonth,
    decimal? Cac,
    IReadOnlyList<SaasDelinquencyItemDto> Delinquency);
