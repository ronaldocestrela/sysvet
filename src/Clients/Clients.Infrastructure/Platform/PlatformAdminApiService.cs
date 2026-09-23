using Clients.Infrastructure.Http;
using Core.Domain;
using Core.Domain.Entitlements;

namespace Clients.Infrastructure.Platform;

/// <summary>Typed HTTP client for Super Admin platform routes.</summary>
public sealed class PlatformAdminApiService : IPlatformAdminApi
{
    private readonly ApiClient _apiClient;

    /// <summary>Creates the service.</summary>
    public PlatformAdminApiService(ApiClient apiClient) => _apiClient = apiClient;

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<PlatformTenantSummaryDto>>> ListTenantsAsync(bool activeOnly = false, CancellationToken cancellationToken = default)
    {
        var all = new List<PlatformTenantSummaryDto>();
        var page = 1;
        while (true)
        {
            var url =
                $"/api/v1/platform/tenants?activeOnly={activeOnly.ToString().ToLowerInvariant()}&page={page}&pageSize=100";
            var pageResult = await _apiClient.GetAsync<PagedResultDto<PlatformTenantSummaryDto>>(url, cancellationToken);
            if (pageResult.IsFailure)
            {
                return Result.Failure<IReadOnlyList<PlatformTenantSummaryDto>>(pageResult.Error);
            }

            all.AddRange(pageResult.Value!.Items);
            if (all.Count >= pageResult.Value.TotalCount || pageResult.Value.Items.Count == 0)
            {
                break;
            }

            page++;
        }

        return Result.Success<IReadOnlyList<PlatformTenantSummaryDto>>(all);
    }

    /// <inheritdoc />
    public Task<Result<PlatformTenantDetailDto>> GetTenantAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        _apiClient.GetAsync<PlatformTenantDetailDto>($"/api/v1/platform/tenants/{tenantId}", cancellationToken);

    /// <inheritdoc />
    public Task<Result<PlatformOnboardTenantResultDto>> OnboardTenantAsync(PlatformOnboardTenantRequest request, CancellationToken cancellationToken = default) =>
        _apiClient.PostAsync<PlatformOnboardTenantRequest, PlatformOnboardTenantResultDto>(
            "/api/v1/platform/tenants",
            request,
            cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<Result> ChangeTenantStatusAsync(Guid tenantId, PlatformTenantStatus status, CancellationToken cancellationToken = default) =>
        _apiClient.PatchAsync(
            $"/api/v1/platform/tenants/{tenantId}/status",
            new ChangeStatusBody(status),
            cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<Result> DeleteTenantAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        _apiClient.DeleteAsync($"/api/v1/platform/tenants/{tenantId}", cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<Result<IReadOnlyList<PlatformBranchDto>>> ListBranchesAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        _apiClient.GetAsync<IReadOnlyList<PlatformBranchDto>>($"/api/v1/platform/tenants/{tenantId}/branches", cancellationToken);

    /// <inheritdoc />
    public Task<Result<PlatformBranchDto>> AddBranchAsync(Guid tenantId, PlatformAddBranchRequest request, CancellationToken cancellationToken = default) =>
        _apiClient.PostAsync<PlatformAddBranchRequest, PlatformBranchDto>(
            $"/api/v1/platform/tenants/{tenantId}/branches",
            request,
            cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<Result> DeleteBranchAsync(Guid tenantId, Guid branchId, CancellationToken cancellationToken = default) =>
        _apiClient.DeleteAsync($"/api/v1/platform/tenants/{tenantId}/branches/{branchId}", cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<Result<IReadOnlyList<PlatformPlanSummaryDto>>> ListPlansAsync(CancellationToken cancellationToken = default) =>
        _apiClient.GetAsync<IReadOnlyList<PlatformPlanSummaryDto>>("/api/v1/platform/plans", cancellationToken);

    /// <inheritdoc />
    public Task<Result<IReadOnlyList<PlatformAddOnSummaryDto>>> ListAddOnsAsync(CancellationToken cancellationToken = default) =>
        _apiClient.GetAsync<IReadOnlyList<PlatformAddOnSummaryDto>>("/api/v1/platform/addons", cancellationToken);

    /// <inheritdoc />
    public Task<Result<PlatformTenantSubscriptionDto>> GetSubscriptionAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        _apiClient.GetAsync<PlatformTenantSubscriptionDto>($"/api/v1/platform/tenants/{tenantId}/subscription", cancellationToken);

    /// <inheritdoc />
    public Task<Result<PlatformPlanChangeResultDto>> ChangePlanAsync(Guid tenantId, string planCode, CancellationToken cancellationToken = default) =>
        _apiClient.PostAsync<ChangePlanBody, PlatformPlanChangeResultDto>(
            $"/api/v1/platform/tenants/{tenantId}/subscription/change",
            new ChangePlanBody(planCode),
            cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<Result<decimal>> ActivateAddOnAsync(Guid tenantId, string addOnCode, CancellationToken cancellationToken = default) =>
        _apiClient.PostAsync<object, decimal>(
            $"/api/v1/platform/tenants/{tenantId}/addons/{Uri.EscapeDataString(addOnCode)}/activate",
            new { },
            cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<Result<decimal>> DeactivateAddOnAsync(Guid tenantId, string addOnCode, CancellationToken cancellationToken = default) =>
        _apiClient.PostAsync<object, decimal>(
            $"/api/v1/platform/tenants/{tenantId}/addons/{Uri.EscapeDataString(addOnCode)}/deactivate",
            new { },
            cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<Result<PlatformTenantEntitlementsDto>> GetEntitlementsAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        _apiClient.GetAsync<PlatformTenantEntitlementsDto>($"/api/v1/platform/tenants/{tenantId}/entitlements", cancellationToken);

    /// <inheritdoc />
    public Task<Result> SetFeatureFlagAsync(Guid tenantId, CommercialModule module, PlatformFeatureFlagState state, CancellationToken cancellationToken = default) =>
        _apiClient.PutAsync(
            $"/api/v1/platform/tenants/{tenantId}/flags/{(int)module}",
            new SetFeatureFlagBody(state),
            cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<Result<PlatformBillingCustomerDto>> UpsertBillingCustomerAsync(
        Guid tenantId,
        PlatformUpsertBillingCustomerRequest request,
        CancellationToken cancellationToken = default) =>
        _apiClient.PutAsync<PlatformUpsertBillingCustomerRequest, PlatformBillingCustomerDto>(
            $"/api/v1/platform/tenants/{tenantId}/billing/customer",
            request,
            cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<Result<PlatformBillingPaymentMethodDto>> UpsertBillingPaymentMethodAsync(
        Guid tenantId,
        PlatformUpsertBillingPaymentMethodRequest request,
        CancellationToken cancellationToken = default) =>
        _apiClient.PutAsync<PlatformUpsertBillingPaymentMethodRequest, PlatformBillingPaymentMethodDto>(
            $"/api/v1/platform/tenants/{tenantId}/billing/payment-method",
            request,
            cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<Result<PlatformChargeBillingResultDto>> ChargeBillingAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        _apiClient.PostAsync<object, PlatformChargeBillingResultDto>(
            $"/api/v1/platform/tenants/{tenantId}/billing/charge",
            new { },
            cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<Result<IReadOnlyList<PlatformBillingInvoiceDto>>> ListBillingInvoicesAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        _apiClient.GetAsync<IReadOnlyList<PlatformBillingInvoiceDto>>(
            $"/api/v1/platform/tenants/{tenantId}/billing/invoices",
            cancellationToken);

    /// <inheritdoc />
    public Task<Result> RedeemCouponAsync(Guid tenantId, string code, CancellationToken cancellationToken = default) =>
        _apiClient.PostAsync(
            $"/api/v1/platform/tenants/{tenantId}/billing/coupon",
            new RedeemCouponBody(code),
            cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<Result<PlatformSaasNfseDto>> RetrySaasNfseAsync(Guid tenantId, Guid invoiceId, CancellationToken cancellationToken = default) =>
        _apiClient.PostAsync<object, PlatformSaasNfseDto>(
            $"/api/v1/platform/tenants/{tenantId}/billing/invoices/{invoiceId}/nfse",
            new { },
            cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<Result<IReadOnlyList<PlatformCouponSummaryDto>>> ListCouponsAsync(CancellationToken cancellationToken = default) =>
        _apiClient.GetAsync<IReadOnlyList<PlatformCouponSummaryDto>>("/api/v1/platform/coupons", cancellationToken);

    /// <inheritdoc />
    public Task<Result<PlatformCouponSummaryDto>> CreateCouponAsync(PlatformCreateCouponRequest request, CancellationToken cancellationToken = default) =>
        _apiClient.PostAsync<PlatformCreateCouponRequest, PlatformCouponSummaryDto>(
            "/api/v1/platform/coupons",
            request,
            cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<Result<PlatformStartImpersonationResultDto>> StartImpersonationAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        _apiClient.PostAsync<object, PlatformStartImpersonationResultDto>(
            $"/api/v1/platform/tenants/{tenantId}/impersonation",
            new { },
            cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<Result> EndImpersonationAsync(Guid sessionId, CancellationToken cancellationToken = default) =>
        _apiClient.PostAsync(
            $"/api/v1/platform/impersonation/{sessionId}/end",
            new { },
            cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<Result<IReadOnlyList<PlatformImpersonationAuditDto>>> ListImpersonationAuditsAsync(int take = 100, CancellationToken cancellationToken = default) =>
        _apiClient.GetAsync<IReadOnlyList<PlatformImpersonationAuditDto>>(
            $"/api/v1/platform/impersonation-audits?take={take}",
            cancellationToken);

    /// <inheritdoc />
    public Task<Result<IReadOnlyList<PlatformLoginLogDto>>> ListLoginLogsAsync(Guid? tenantId, int take = 100, CancellationToken cancellationToken = default)
    {
        var query = tenantId is { } id
            ? $"?tenantId={id}&take={take}"
            : $"?take={take}";
        return _apiClient.GetAsync<IReadOnlyList<PlatformLoginLogDto>>($"/api/v1/platform/login-logs{query}", cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result<IReadOnlyList<PlatformChangeAuditDto>>> ListChangeAuditsAsync(Guid? tenantId, int take = 100, CancellationToken cancellationToken = default)
    {
        var query = tenantId is { } id
            ? $"?tenantId={id}&take={take}"
            : $"?take={take}";
        return _apiClient.GetAsync<IReadOnlyList<PlatformChangeAuditDto>>($"/api/v1/platform/change-audits{query}", cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result<PlatformTenantHealthDto>> GetTenantHealthAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        _apiClient.GetAsync<PlatformTenantHealthDto>($"/api/v1/platform/tenants/{tenantId}/health", cancellationToken);

    /// <inheritdoc />
    public Task<Result<IReadOnlyList<PlatformPartnerApiKeySummaryDto>>> ListPartnerApiKeysAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        _apiClient.GetAsync<IReadOnlyList<PlatformPartnerApiKeySummaryDto>>(
            $"/api/v1/platform/tenants/{tenantId}/api-keys",
            cancellationToken);

    /// <inheritdoc />
    public Task<Result<PlatformCreatePartnerApiKeyResultDto>> CreatePartnerApiKeyAsync(
        Guid tenantId,
        string partnerName,
        CancellationToken cancellationToken = default) =>
        _apiClient.PostAsync<CreatePartnerApiKeyBody, PlatformCreatePartnerApiKeyResultDto>(
            $"/api/v1/platform/tenants/{tenantId}/api-keys",
            new CreatePartnerApiKeyBody(partnerName),
            cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<Result> RevokePartnerApiKeyAsync(Guid tenantId, Guid keyId, CancellationToken cancellationToken = default) =>
        _apiClient.PostAsync(
            $"/api/v1/platform/tenants/{tenantId}/api-keys/{keyId}/revoke",
            new { },
            cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<Result<PlatformSaasMetricsDto>> GetSaasMetricsAsync(int year, int month, CancellationToken cancellationToken = default) =>
        _apiClient.GetAsync<PlatformSaasMetricsDto>(
            $"/api/v1/platform/metrics?year={year}&month={month}",
            cancellationToken);

    /// <inheritdoc />
    public Task<Result> UpsertAcquisitionSpendAsync(PlatformUpsertAcquisitionSpendRequest request, CancellationToken cancellationToken = default) =>
        _apiClient.PostAsync(
            "/api/v1/platform/metrics/acquisition-spend",
            request,
            cancellationToken: cancellationToken);

    /// <inheritdoc />
    public Task<Result<PlatformModuleAdoptionDto>> GetModuleAdoptionAsync(CancellationToken cancellationToken = default) =>
        _apiClient.GetAsync<PlatformModuleAdoptionDto>("/api/v1/platform/adoption", cancellationToken);

    /// <inheritdoc />
    public Task<Result<DownloadedFile>> ExportModuleAdoptionAsync(CancellationToken cancellationToken = default) =>
        _apiClient.DownloadGetAsync("/api/v1/platform/adoption/export", cancellationToken);

    private sealed record ChangeStatusBody(PlatformTenantStatus Status);
    private sealed record ChangePlanBody(string PlanCode);
    private sealed record SetFeatureFlagBody(PlatformFeatureFlagState State);
    private sealed record RedeemCouponBody(string Code);
    private sealed record CreatePartnerApiKeyBody(string PartnerName);
}
