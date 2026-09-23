using Clients.Infrastructure.Platform;
using Core.Domain;
using Core.Domain.Entitlements;
using PlatformWeb.Services;

namespace Clients.Tests.PlatformWeb;

/// <summary>Test double for platform admin HTTP calls.</summary>
public sealed class FakePlatformAdminApi : IPlatformAdminApi
{
    public PlatformOnboardTenantRequest? LastOnboardRequest { get; private set; }
    public (Guid TenantId, CommercialModule Module, PlatformFeatureFlagState State)? LastFlag { get; private set; }
    public IReadOnlyList<PlatformTenantSummaryDto> Tenants { get; set; } =
    [
        new(Guid.NewGuid(), "clinica-a", "Clínica A", PlatformTenantStatus.Active, "tenant_a", DateTimeOffset.UtcNow)
    ];

    public Task<Result<IReadOnlyList<PlatformTenantSummaryDto>>> ListTenantsAsync(bool activeOnly = false, CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Success(Tenants));

    public Task<Result<PlatformOnboardTenantResultDto>> OnboardTenantAsync(PlatformOnboardTenantRequest request, CancellationToken cancellationToken = default)
    {
        LastOnboardRequest = request;
        return Task.FromResult(Result.Success(new PlatformOnboardTenantResultDto(Guid.NewGuid(), "user", request.Slug)));
    }

    public Task<Result> SetFeatureFlagAsync(Guid tenantId, CommercialModule module, PlatformFeatureFlagState state, CancellationToken cancellationToken = default)
    {
        LastFlag = (tenantId, module, state);
        return Task.FromResult(Result.Success());
    }

    public Task<Result<PlatformCreatePartnerApiKeyResultDto>> CreatePartnerApiKeyAsync(Guid tenantId, string partnerName, CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Success(new PlatformCreatePartnerApiKeyResultDto(
            Guid.NewGuid(),
            partnerName,
            "pk_test",
            "secret-once-123",
            "partner",
            DateTimeOffset.UtcNow)));

    public Task<Result<IReadOnlyList<PlatformPartnerApiKeySummaryDto>>> ListPartnerApiKeysAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Success<IReadOnlyList<PlatformPartnerApiKeySummaryDto>>([]));

    public Task<Result> RevokePartnerApiKeyAsync(Guid tenantId, Guid keyId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Success());

    public Task<Result<PlatformTenantDetailDto>> GetTenantAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task<Result> ChangeTenantStatusAsync(Guid tenantId, PlatformTenantStatus status, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task<Result> DeleteTenantAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task<Result<IReadOnlyList<PlatformBranchDto>>> ListBranchesAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task<Result<PlatformBranchDto>> AddBranchAsync(Guid tenantId, PlatformAddBranchRequest request, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task<Result> DeleteBranchAsync(Guid tenantId, Guid branchId, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task<Result<IReadOnlyList<PlatformPlanSummaryDto>>> ListPlansAsync(CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task<Result<IReadOnlyList<PlatformAddOnSummaryDto>>> ListAddOnsAsync(CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task<Result<PlatformTenantSubscriptionDto>> GetSubscriptionAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task<Result<PlatformPlanChangeResultDto>> ChangePlanAsync(Guid tenantId, string planCode, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task<Result<decimal>> ActivateAddOnAsync(Guid tenantId, string addOnCode, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task<Result<decimal>> DeactivateAddOnAsync(Guid tenantId, string addOnCode, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task<Result<PlatformTenantEntitlementsDto>> GetEntitlementsAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task<Result<PlatformBillingCustomerDto>> UpsertBillingCustomerAsync(Guid tenantId, PlatformUpsertBillingCustomerRequest request, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task<Result<PlatformBillingPaymentMethodDto>> UpsertBillingPaymentMethodAsync(Guid tenantId, PlatformUpsertBillingPaymentMethodRequest request, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task<Result<PlatformChargeBillingResultDto>> ChargeBillingAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task<Result<IReadOnlyList<PlatformBillingInvoiceDto>>> ListBillingInvoicesAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task<Result> RedeemCouponAsync(Guid tenantId, string code, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task<Result<PlatformSaasNfseDto>> RetrySaasNfseAsync(Guid tenantId, Guid invoiceId, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task<Result<IReadOnlyList<PlatformCouponSummaryDto>>> ListCouponsAsync(CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task<Result<PlatformCouponSummaryDto>> CreateCouponAsync(PlatformCreateCouponRequest request, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task<Result<PlatformStartImpersonationResultDto>> StartImpersonationAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task<Result> EndImpersonationAsync(Guid sessionId, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task<Result<IReadOnlyList<PlatformImpersonationAuditDto>>> ListImpersonationAuditsAsync(int take = 100, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task<Result<IReadOnlyList<PlatformLoginLogDto>>> ListLoginLogsAsync(Guid? tenantId, int take = 100, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task<Result<IReadOnlyList<PlatformChangeAuditDto>>> ListChangeAuditsAsync(Guid? tenantId, int take = 100, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task<Result<PlatformTenantHealthDto>> GetTenantHealthAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public PlatformSaasMetricsDto Metrics { get; set; } = new(
        2026,
        3,
        600m,
        1990m,
        7200m,
        300m,
        50m,
        250m,
        0.1m,
        3,
        1,
        10,
        2000m,
        1000m,
        2,
        500m,
        []);

    public PlatformUpsertAcquisitionSpendRequest? LastAcquisitionSpend { get; private set; }

    public Task<Result<PlatformSaasMetricsDto>> GetSaasMetricsAsync(int year, int month, CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Success(Metrics with { Year = year, Month = month }));

    public Task<Result> UpsertAcquisitionSpendAsync(PlatformUpsertAcquisitionSpendRequest request, CancellationToken cancellationToken = default)
    {
        LastAcquisitionSpend = request;
        return Task.FromResult(Result.Success());
    }
}

/// <summary>Configurable auth state for platform UI tests.</summary>
public sealed class FakePlatformAuthState : IPlatformAuthState
{
    public bool IsAuthenticated { get; set; } = true;
    public IReadOnlyList<string> Menus { get; private set; } = [];
    public decimal MaxDiscountPercent => 0;
    public IReadOnlyList<string> Roles { get; set; } = [PlatformRoles.SuperAdmin];
    public bool IsSuperAdmin => Roles.Contains(PlatformRoles.SuperAdmin, StringComparer.Ordinal);
    public event EventHandler? SessionChanged;

    public Task InitializeAsync() => Task.CompletedTask;
    public Task<string?> GetTokenAsync() => Task.FromResult<string?>("token");
    public Task<string?> GetRefreshTokenAsync() => Task.FromResult<string?>("refresh");
    public Task LoginAsync(string accessToken, string refreshToken) => Task.CompletedTask;
    public Task SetMenusAsync(IReadOnlyList<string> menus) { Menus = menus; return Task.CompletedTask; }
    public Task SetRolesAsync(IReadOnlyList<string> roles) { Roles = roles; return Task.CompletedTask; }
    public Task LogoutAsync() => Task.CompletedTask;
}
