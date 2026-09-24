using Clients.Infrastructure.Http;
using Core.Domain;
using Core.Domain.Entitlements;

namespace Clients.Infrastructure.Platform;

/// <summary>HTTP contract for Super Admin platform API (roadmap 9.8).</summary>
public interface IPlatformAdminApi
{
    Task<Result<IReadOnlyList<PlatformTenantSummaryDto>>> ListTenantsAsync(bool activeOnly = false, CancellationToken cancellationToken = default);
    Task<Result<PlatformTenantDetailDto>> GetTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<Result<PlatformOnboardTenantResultDto>> OnboardTenantAsync(PlatformOnboardTenantRequest request, CancellationToken cancellationToken = default);
    Task<Result> ChangeTenantStatusAsync(Guid tenantId, PlatformTenantStatus status, CancellationToken cancellationToken = default);
    Task<Result> DeleteTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<PlatformBranchDto>>> ListBranchesAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<Result<PlatformBranchDto>> AddBranchAsync(Guid tenantId, PlatformAddBranchRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteBranchAsync(Guid tenantId, Guid branchId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<PlatformPlanSummaryDto>>> ListPlansAsync(CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<PlatformAddOnSummaryDto>>> ListAddOnsAsync(CancellationToken cancellationToken = default);
    Task<Result<PlatformTenantSubscriptionDto>> GetSubscriptionAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<Result<PlatformPlanChangeResultDto>> ChangePlanAsync(Guid tenantId, string planCode, CancellationToken cancellationToken = default);
    Task<Result<decimal>> ActivateAddOnAsync(Guid tenantId, string addOnCode, CancellationToken cancellationToken = default);
    Task<Result<decimal>> DeactivateAddOnAsync(Guid tenantId, string addOnCode, CancellationToken cancellationToken = default);
    Task<Result<PlatformTenantEntitlementsDto>> GetEntitlementsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<Result> SetFeatureFlagAsync(Guid tenantId, CommercialModule module, PlatformFeatureFlagState state, CancellationToken cancellationToken = default);
    Task<Result> SetReleaseRingAsync(Guid tenantId, PlatformReleaseRing ring, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<PlatformStatusIncidentDto>>> ListStatusIncidentsAsync(int take = 50, CancellationToken cancellationToken = default);
    Task<Result<PlatformStatusIncidentDto>> CreateStatusIncidentAsync(PlatformCreateStatusIncidentRequest request, CancellationToken cancellationToken = default);
    Task<Result<PlatformStatusIncidentDto>> ResolveStatusIncidentAsync(Guid incidentId, CancellationToken cancellationToken = default);
    Task<Result<PlatformBillingCustomerDto>> UpsertBillingCustomerAsync(Guid tenantId, PlatformUpsertBillingCustomerRequest request, CancellationToken cancellationToken = default);
    Task<Result<PlatformBillingPaymentMethodDto>> UpsertBillingPaymentMethodAsync(Guid tenantId, PlatformUpsertBillingPaymentMethodRequest request, CancellationToken cancellationToken = default);
    Task<Result<PlatformChargeBillingResultDto>> ChargeBillingAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<PlatformBillingInvoiceDto>>> ListBillingInvoicesAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<Result> RedeemCouponAsync(Guid tenantId, string code, CancellationToken cancellationToken = default);
    Task<Result<PlatformSaasNfseDto>> RetrySaasNfseAsync(Guid tenantId, Guid invoiceId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<PlatformCouponSummaryDto>>> ListCouponsAsync(CancellationToken cancellationToken = default);
    Task<Result<PlatformCouponSummaryDto>> CreateCouponAsync(PlatformCreateCouponRequest request, CancellationToken cancellationToken = default);
    Task<Result<PlatformStartImpersonationResultDto>> StartImpersonationAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<Result> EndImpersonationAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<PlatformImpersonationAuditDto>>> ListImpersonationAuditsAsync(int take = 100, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<PlatformLoginLogDto>>> ListLoginLogsAsync(Guid? tenantId, int take = 100, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<PlatformChangeAuditDto>>> ListChangeAuditsAsync(Guid? tenantId, int take = 100, CancellationToken cancellationToken = default);
    Task<Result<PlatformTenantHealthDto>> GetTenantHealthAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<PlatformPartnerApiKeySummaryDto>>> ListPartnerApiKeysAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<Result<PlatformCreatePartnerApiKeyResultDto>> CreatePartnerApiKeyAsync(Guid tenantId, string partnerName, CancellationToken cancellationToken = default);
    Task<Result> RevokePartnerApiKeyAsync(Guid tenantId, Guid keyId, CancellationToken cancellationToken = default);
    Task<Result<PlatformSaasMetricsDto>> GetSaasMetricsAsync(int year, int month, CancellationToken cancellationToken = default);
    Task<Result> UpsertAcquisitionSpendAsync(PlatformUpsertAcquisitionSpendRequest request, CancellationToken cancellationToken = default);
    Task<Result<PlatformModuleAdoptionDto>> GetModuleAdoptionAsync(CancellationToken cancellationToken = default);
    Task<Result<DownloadedFile>> ExportModuleAdoptionAsync(CancellationToken cancellationToken = default);
}

/// <summary>Tenant list row.</summary>
public sealed record PlatformTenantSummaryDto(
    Guid Id,
    string Slug,
    string DisplayName,
    PlatformTenantStatus Status,
    PlatformReleaseRing ReleaseRing,
    string SchemaName,
    DateTimeOffset UpdatedAt);

/// <summary>Tenant detail.</summary>
public sealed record PlatformTenantDetailDto(
    Guid Id,
    string Slug,
    string DisplayName,
    PlatformTenantStatus Status,
    PlatformReleaseRing ReleaseRing,
    string SchemaName,
    DateTimeOffset UpdatedAt,
    int BranchCount);

/// <summary>Status incident row.</summary>
public sealed record PlatformStatusIncidentDto(
    Guid Id,
    string Title,
    PlatformStatusIncidentImpact Impact,
    string Components,
    DateTimeOffset StartedAt,
    DateTimeOffset? ResolvedAt);

/// <summary>Create status incident body.</summary>
public sealed record PlatformCreateStatusIncidentRequest(
    string Title,
    PlatformStatusIncidentImpact Impact,
    string Components);

/// <summary>Onboarding request body.</summary>
public sealed record PlatformOnboardTenantRequest(
    string Slug,
    string DisplayName,
    string AdminEmail,
    string AdminPassword,
    string HeadquartersCnpj,
    string HeadquartersLegalName,
    string? PlanCode = null,
    int? TrialDays = null);

/// <summary>Onboarding success payload.</summary>
public sealed record PlatformOnboardTenantResultDto(Guid TenantId, string AdminUserId, string Slug);

/// <summary>Branch row.</summary>
public sealed record PlatformBranchDto(
    Guid Id,
    Guid TenantId,
    string Cnpj,
    string LegalName,
    bool IsHeadquarters);

/// <summary>Add branch body.</summary>
public sealed record PlatformAddBranchRequest(string Cnpj, string LegalName, bool IsHeadquarters = false);

/// <summary>Plan catalog row.</summary>
public sealed record PlatformPlanSummaryDto(
    Guid Id,
    string Code,
    string Name,
    decimal MonthlyPrice,
    IReadOnlyList<CommercialModule> Modules);

/// <summary>Add-on catalog row.</summary>
public sealed record PlatformAddOnSummaryDto(
    Guid Id,
    string Code,
    string Name,
    decimal MonthlyPrice,
    CommercialModule Module);

/// <summary>Tenant subscription view.</summary>
public sealed record PlatformTenantSubscriptionDto(
    Guid TenantId,
    Guid PlanId,
    string PlanCode,
    int Status,
    DateTimeOffset PeriodStart,
    DateTimeOffset PeriodEnd,
    DateTimeOffset? TrialEndsAt,
    int TrialEndAction,
    decimal CreditBalance,
    IReadOnlyList<string> ActiveAddOnCodes);

/// <summary>Effective modules for tenant.</summary>
public sealed record PlatformTenantEntitlementsDto(Guid TenantId, IReadOnlyList<CommercialModule> Modules);

/// <summary>Plan change proration result.</summary>
public sealed record PlatformPlanChangeResultDto(decimal ProrationAmount, Guid NewPlanId);

/// <summary>Billing customer view.</summary>
public sealed record PlatformBillingCustomerDto(
    Guid TenantId,
    string Name,
    string Email,
    string CpfCnpj,
    string? GatewayCustomerId);

/// <summary>Payment method view.</summary>
public sealed record PlatformBillingPaymentMethodDto(
    Guid TenantId,
    PlatformBillingPaymentMethodKind Kind,
    bool HasCreditCardToken);

/// <summary>Upsert billing customer body.</summary>
public sealed record PlatformUpsertBillingCustomerRequest(string Name, string Email, string CpfCnpj);

/// <summary>Upsert payment method body.</summary>
public sealed record PlatformUpsertBillingPaymentMethodRequest(PlatformBillingPaymentMethodKind Kind, string? CreditCardToken);

/// <summary>Charge cycle result.</summary>
public sealed record PlatformChargeBillingResultDto(
    Guid InvoiceId,
    decimal Amount,
    PlatformBillingInvoiceStatus Status,
    string? GatewayPaymentId);

/// <summary>Invoice list item.</summary>
public sealed record PlatformBillingInvoiceDto(
    Guid Id,
    DateTimeOffset PeriodStart,
    DateTimeOffset PeriodEnd,
    decimal Amount,
    PlatformBillingInvoiceStatus Status,
    DateTimeOffset? PaidAt);

/// <summary>Coupon list item.</summary>
public sealed record PlatformCouponSummaryDto(
    Guid Id,
    string Code,
    PlatformCouponDiscountType DiscountType,
    decimal Value,
    int? MaxRedemptions,
    int RedemptionCount,
    DateTimeOffset? ExpiresAt,
    bool IsActive);

/// <summary>Create coupon body.</summary>
public sealed record PlatformCreateCouponRequest(
    string Code,
    PlatformCouponDiscountType DiscountType,
    decimal Value,
    int? MaxRedemptions,
    DateTimeOffset? ExpiresAt);

/// <summary>Impersonation start result.</summary>
public sealed record PlatformStartImpersonationResultDto(
    string AccessToken,
    int ExpiresInSeconds,
    Guid SessionId,
    Guid TargetTenantId);

/// <summary>Impersonation audit row.</summary>
public sealed record PlatformImpersonationAuditDto(
    Guid Id,
    Guid SessionId,
    string ActorUserId,
    Guid TargetTenantId,
    string Action,
    DateTimeOffset OccurredAt,
    string ClientIp);

/// <summary>Login log row.</summary>
public sealed record PlatformLoginLogDto(
    Guid Id,
    Guid? TenantId,
    string Email,
    bool Succeeded,
    string ClientIp,
    string UserAgent,
    string Country,
    string Region,
    DateTimeOffset OccurredAt);

/// <summary>Change audit row.</summary>
public sealed record PlatformChangeAuditDto(
    Guid Id,
    string ActorUserId,
    Guid? TenantId,
    string Action,
    string PayloadSummary,
    string ClientIp,
    DateTimeOffset OccurredAt);

/// <summary>Tenant health dashboard.</summary>
public sealed record PlatformTenantHealthDto(
    Guid TenantId,
    IReadOnlyList<PlatformDataVolumeSliceDto> DataVolume,
    long TotalRowCount,
    long EstimatedBytes,
    int RequestsToday,
    int RequestsLast7Days,
    DateTimeOffset GeneratedAtUtc);

/// <summary>Health data volume slice.</summary>
public sealed record PlatformDataVolumeSliceDto(string Module, long RowCount);

/// <summary>Partner API key summary.</summary>
public sealed record PlatformPartnerApiKeySummaryDto(
    Guid KeyId,
    string PartnerName,
    string KeyPrefix,
    string Scope,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? RevokedAt);

/// <summary>Created API key with one-time secret.</summary>
public sealed record PlatformCreatePartnerApiKeyResultDto(
    Guid KeyId,
    string PartnerName,
    string KeyPrefix,
    string Secret,
    string Scope,
    DateTimeOffset CreatedAt);

/// <summary>NFS-e retry result.</summary>
public sealed record PlatformSaasNfseDto(int Status, string? NfseNumber, string? FailureReason);

/// <summary>Delinquent tenant row (10.2).</summary>
public sealed record PlatformSaasDelinquencyItemDto(
    Guid TenantId,
    string DisplayName,
    decimal OutstandingAmount,
    DateTimeOffset? PastDueSince,
    int BillingStanding);

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
    IReadOnlyList<PlatformSaasDelinquencyItemDto> Delinquency);

/// <summary>Acquisition spend upsert body (10.2).</summary>
public sealed record PlatformUpsertAcquisitionSpendRequest(
    int Year,
    int Month,
    string Channel,
    decimal Amount,
    string? Note);

/// <summary>Module adoption heatmap (10.3).</summary>
public sealed record PlatformModuleAdoptionDto(
    IReadOnlyList<PlatformTenantModuleAdoptionRowDto> Tenants,
    IReadOnlyList<PlatformModuleAdoptionSummaryDto> ModuleSummaries);

/// <summary>Tenant row in adoption matrix.</summary>
public sealed record PlatformTenantModuleAdoptionRowDto(
    Guid TenantId,
    string DisplayName,
    IReadOnlyList<PlatformModuleAdoptionCellDto> Cells);

/// <summary>Cell in adoption matrix.</summary>
public sealed record PlatformModuleAdoptionCellDto(CommercialModule Module, bool Enabled);

/// <summary>Module adoption summary.</summary>
public sealed record PlatformModuleAdoptionSummaryDto(CommercialModule Module, int EnabledCount, decimal AdoptionRate);
