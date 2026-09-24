using Core.Application.Authorization;
using Core.Application.Common;
using Core.Domain;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Core.Domain.Entitlements;
using Platform.Application.Billing;
using Platform.Application.Coupons;
using Platform.Application.Subscriptions;
using Platform.Application.Tenants.Commands;
using Platform.Application.Tenants.Dtos;
using Platform.Application.Tenants.Queries;
using Platform.Application.Impersonation;
using Platform.Application.Auditing;
using Platform.Application.ApiKeys;
using Platform.Application.Health;
using Platform.Application.Metrics;
using Platform.Application.Status;
using Platform.Domain.Entities;

namespace API.Extensions;

/// <summary>Super Admin platform tenant management endpoints (roadmap 9.2).</summary>
public static class PlatformEndpointsExtensions
{
    /// <summary>Maps Super Admin platform routes (9.2–9.3).</summary>
    public static IEndpointRouteBuilder MapPlatformEndpoints(this IEndpointRouteBuilder builder)
    {
        MapPlatformCatalogEndpoints(builder);
        return MapPlatformTenantEndpoints(builder);
    }

    private static void MapPlatformCatalogEndpoints(IEndpointRouteBuilder builder)
    {
        var catalog = builder.MapGroup("/api/v1/platform")
            .WithTags("Platform")
            .RequireAuthorization(AuthorizationPolicies.PlatformAdmin);

        catalog.MapGet("/plans", ListPlans);
        catalog.MapGet("/addons", ListAddOns);
        catalog.MapGet("/coupons", ListCoupons);
        catalog.MapPost("/coupons", CreateCoupon);
        catalog.MapGet("/impersonation-audits", ListImpersonationAudits);
        catalog.MapPost("/impersonation/{sessionId:guid}/end", EndImpersonation);
        catalog.MapGet("/login-logs", ListPlatformLoginLogs);
        catalog.MapGet("/change-audits", ListPlatformChangeAudits);
        catalog.MapGet("/metrics", GetPlatformSaasMetrics);
        catalog.MapPost("/metrics/acquisition-spend", UpsertAcquisitionSpend);
        catalog.MapGet("/adoption", async (IMediator mediator) =>
            (await mediator.Send(new GetPlatformModuleAdoptionQuery())).ToHttpResult());
        catalog.MapGet("/adoption/export", async (IMediator mediator) =>
            ToAdoptionFileResult(await mediator.Send(new ExportPlatformModuleAdoptionQuery())));

        catalog.MapGet("/status/incidents", ListStatusIncidents);
        catalog.MapPost("/status/incidents", CreateStatusIncident);
        catalog.MapPatch("/status/incidents/{incidentId:guid}/resolve", ResolveStatusIncident);
    }

    /// <summary>Maps <c>/api/v1/platform/tenants</c> routes.</summary>
    public static IEndpointRouteBuilder MapPlatformTenantEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/v1/platform/tenants")
            .WithTags("Platform")
            .RequireAuthorization(AuthorizationPolicies.PlatformAdmin);

        group.MapPost("/", OnboardTenant);
        group.MapGet("/", ListTenants);
        group.MapGet("/{tenantId:guid}", GetTenant);
        group.MapPatch("/{tenantId:guid}/status", ChangeStatus);
        group.MapPatch("/{tenantId:guid}/release-ring", SetReleaseRing);
        group.MapDelete("/{tenantId:guid}", DeleteTenant);

        group.MapGet("/{tenantId:guid}/branches", ListBranches);
        group.MapPost("/{tenantId:guid}/branches", AddBranch);
        group.MapPatch("/{tenantId:guid}/branches/{branchId:guid}", UpdateBranch);
        group.MapDelete("/{tenantId:guid}/branches/{branchId:guid}", DeleteBranch);

        group.MapGet("/{tenantId:guid}/subscription", GetSubscription);
        group.MapPost("/{tenantId:guid}/subscription/change", ChangePlan);
        group.MapPost("/{tenantId:guid}/addons/{addOnCode}/activate", ActivateAddOn);
        group.MapPost("/{tenantId:guid}/addons/{addOnCode}/deactivate", DeactivateAddOn);
        group.MapGet("/{tenantId:guid}/entitlements", GetEntitlements);
        group.MapPut("/{tenantId:guid}/flags/{module}", SetFeatureFlag);

        group.MapPut("/{tenantId:guid}/billing/customer", UpsertBillingCustomer);
        group.MapPut("/{tenantId:guid}/billing/payment-method", UpsertBillingPaymentMethod);
        group.MapPost("/{tenantId:guid}/billing/charge", ChargeTenantBilling);
        group.MapGet("/{tenantId:guid}/billing/invoices", ListBillingInvoices);
        group.MapPost("/{tenantId:guid}/billing/coupon", RedeemCoupon);
        group.MapPost("/{tenantId:guid}/billing/invoices/{invoiceId:guid}/nfse", RetrySaasNfse);
        group.MapPost("/{tenantId:guid}/impersonation", StartImpersonation);
        group.MapGet("/{tenantId:guid}/health", GetTenantHealth);
        group.MapGet("/{tenantId:guid}/api-keys", ListPartnerApiKeys);
        group.MapPost("/{tenantId:guid}/api-keys", CreatePartnerApiKey);
        group.MapPost("/{tenantId:guid}/api-keys/{keyId:guid}/revoke", RevokePartnerApiKey);

        return builder;
    }

    private static Task<Result<IReadOnlyList<CouponSummaryDto>>> ListCoupons(IMediator mediator) =>
        mediator.Send(new ListCouponsQuery());

    private static Task<Result<CouponSummaryDto>> CreateCoupon([FromBody] CreateCouponRequest body, IMediator mediator) =>
        mediator.Send(new CreateCouponCommand(
            body.Code,
            body.DiscountType,
            body.Value,
            body.MaxRedemptions,
            body.ExpiresAt));

    private static Task<Result> RedeemCoupon(
        Guid tenantId,
        [FromBody] RedeemCouponRequest body,
        IMediator mediator) =>
        mediator.Send(new RedeemTenantCouponCommand(tenantId, body.Code));

    private static Task<Result<IReadOnlyList<PlanSummaryDto>>> ListPlans(IMediator mediator) =>
        mediator.Send(new ListPlansQuery());

    private static Task<Result<IReadOnlyList<AddOnSummaryDto>>> ListAddOns(IMediator mediator) =>
        mediator.Send(new ListAddOnsQuery());

    private static Task<Result<TenantSubscriptionDto>> GetSubscription(Guid tenantId, IMediator mediator) =>
        mediator.Send(new GetTenantSubscriptionQuery(tenantId));

    private static Task<Result<PlanChangeResultDto>> ChangePlan(Guid tenantId, [FromBody] ChangePlanRequest body, IMediator mediator) =>
        mediator.Send(new ChangeTenantPlanCommand(tenantId, body.PlanCode));

    private static Task<Result<decimal>> ActivateAddOn(Guid tenantId, string addOnCode, IMediator mediator) =>
        mediator.Send(new ActivateTenantAddOnCommand(tenantId, addOnCode));

    private static Task<Result<decimal>> DeactivateAddOn(Guid tenantId, string addOnCode, IMediator mediator) =>
        mediator.Send(new DeactivateTenantAddOnCommand(tenantId, addOnCode));

    private static Task<Result<TenantEntitlementsDto>> GetEntitlements(Guid tenantId, IMediator mediator) =>
        mediator.Send(new GetTenantEntitlementsQuery(tenantId));

    private static Task<Result> SetFeatureFlag(Guid tenantId, CommercialModule module, [FromBody] SetFeatureFlagRequest body, IMediator mediator) =>
        mediator.Send(new SetTenantFeatureFlagCommand(tenantId, module, body.State));

    private static Task<Result<BillingCustomerDto>> UpsertBillingCustomer(
        Guid tenantId,
        [FromBody] UpsertBillingCustomerRequest body,
        IMediator mediator) =>
        mediator.Send(new UpsertBillingCustomerCommand(tenantId, body.Name, body.Email, body.CpfCnpj));

    private static Task<Result<BillingPaymentMethodDto>> UpsertBillingPaymentMethod(
        Guid tenantId,
        [FromBody] UpsertBillingPaymentMethodRequest body,
        IMediator mediator) =>
        mediator.Send(new UpsertBillingPaymentMethodCommand(tenantId, body.Kind, body.CreditCardToken));

    private static Task<Result<ChargeTenantBillingResultDto>> ChargeTenantBilling(Guid tenantId, IMediator mediator) =>
        mediator.Send(new ChargeTenantBillingCommand(tenantId, DateTimeOffset.UtcNow));

    private static Task<Result<PagedResult<BillingInvoiceDto>>> ListBillingInvoices(
        Guid tenantId,
        [FromQuery] int page,
        [FromQuery] int pageSize,
        IMediator mediator) =>
        mediator.Send(new ListTenantBillingInvoicesQuery(tenantId, page <= 0 ? 1 : page, pageSize));

    private static Task<Result<SaasNfseDto>> RetrySaasNfse(Guid tenantId, Guid invoiceId, IMediator mediator) =>
        mediator.Send(new RetrySaasNfseForInvoiceCommand(tenantId, invoiceId));

    private static Task<Result<StartImpersonationResultDto>> StartImpersonation(
        Guid tenantId,
        HttpContext httpContext,
        IMediator mediator) =>
        mediator.Send(new StartImpersonationCommand(tenantId, ResolveClientIp(httpContext)));

    private static Task<Result> EndImpersonation(Guid sessionId, HttpContext httpContext, IMediator mediator) =>
        mediator.Send(new EndImpersonationCommand(sessionId, ResolveClientIp(httpContext)));

    private static Task<Result<PagedResult<ImpersonationAuditDto>>> ListImpersonationAudits(
        [FromQuery] int page,
        [FromQuery] int pageSize,
        IMediator mediator) =>
        mediator.Send(new ListImpersonationAuditsQuery(page <= 0 ? 1 : page, pageSize));

    private static Task<Result<PagedResult<PlatformLoginLogDto>>> ListPlatformLoginLogs(
        [FromQuery] Guid? tenantId,
        [FromQuery] int page,
        [FromQuery] int pageSize,
        IMediator mediator) =>
        mediator.Send(new ListPlatformLoginLogsQuery(tenantId, page <= 0 ? 1 : page, pageSize));

    private static Task<Result<PagedResult<PlatformChangeAuditDto>>> ListPlatformChangeAudits(
        [FromQuery] Guid? tenantId,
        [FromQuery] int page,
        [FromQuery] int pageSize,
        IMediator mediator) =>
        mediator.Send(new ListPlatformChangeAuditsQuery(tenantId, page <= 0 ? 1 : page, pageSize));

    private static Task<Result<TenantHealthDto>> GetTenantHealth(Guid tenantId, IMediator mediator) =>
        mediator.Send(new GetTenantHealthQuery(tenantId));

    private static Task<Result<IReadOnlyList<PartnerApiKeySummaryDto>>> ListPartnerApiKeys(Guid tenantId, IMediator mediator) =>
        mediator.Send(new ListPartnerApiKeysQuery(tenantId));

    private static Task<Result<CreatePartnerApiKeyResultDto>> CreatePartnerApiKey(
        Guid tenantId,
        [FromBody] CreatePartnerApiKeyRequest body,
        IMediator mediator) =>
        mediator.Send(new CreatePartnerApiKeyCommand(tenantId, body.PartnerName));

    private static Task<Result> RevokePartnerApiKey(Guid tenantId, Guid keyId, IMediator mediator) =>
        mediator.Send(new RevokePartnerApiKeyCommand(tenantId, keyId));

    private static string ResolveClientIp(HttpContext context) =>
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    private static Task<Result<OnboardTenantResultDto>> OnboardTenant([FromBody] OnboardTenantCommand command, IMediator mediator) =>
        mediator.Send(command);

    private static Task<Result<PagedResult<TenantSummaryDto>>> ListTenants(
        [FromQuery] bool activeOnly,
        [FromQuery] int page,
        [FromQuery] int pageSize,
        IMediator mediator) =>
        mediator.Send(new ListTenantsQuery(activeOnly, page <= 0 ? 1 : page, pageSize));

    private static Task<Result<TenantDetailDto>> GetTenant(Guid tenantId, IMediator mediator) =>
        mediator.Send(new GetTenantQuery(tenantId));

    private static Task<Result> ChangeStatus(Guid tenantId, [FromBody] ChangeStatusRequest body, IMediator mediator) =>
        mediator.Send(new ChangeTenantStatusCommand(tenantId, body.Status));

    private static Task<Result> SetReleaseRing(
        Guid tenantId,
        [FromBody] SetReleaseRingRequest body,
        IMediator mediator) =>
        mediator.Send(new SetTenantReleaseRingCommand(tenantId, body.Ring));

    private static Task<Result<IReadOnlyList<StatusIncidentDto>>> ListStatusIncidents(
        [FromQuery] int take,
        IMediator mediator) =>
        mediator.Send(new ListStatusIncidentsQuery(take <= 0 ? 50 : take));

    private static Task<Result<StatusIncidentDto>> CreateStatusIncident(
        [FromBody] CreateStatusIncidentRequest body,
        IMediator mediator) =>
        mediator.Send(new CreateStatusIncidentCommand(body.Title, body.Impact, body.Components));

    private static Task<Result<StatusIncidentDto>> ResolveStatusIncident(Guid incidentId, IMediator mediator) =>
        mediator.Send(new ResolveStatusIncidentCommand(incidentId));

    private static Task<Result> DeleteTenant(Guid tenantId, IMediator mediator) =>
        mediator.Send(new DeleteTenantCommand(tenantId));

    private static Task<Result<IReadOnlyList<BranchDto>>> ListBranches(Guid tenantId, IMediator mediator) =>
        mediator.Send(new ListBranchesQuery(tenantId));

    private static Task<Result<BranchDto>> AddBranch(Guid tenantId, [FromBody] AddBranchRequest body, IMediator mediator) =>
        mediator.Send(new AddBranchCommand(tenantId, body.Cnpj, body.LegalName, body.IsHeadquarters));

    private static Task<Result<BranchDto>> UpdateBranch(Guid tenantId, Guid branchId, [FromBody] UpdateBranchRequest body, IMediator mediator) =>
        mediator.Send(new UpdateBranchCommand(tenantId, branchId, body.LegalName));

    private static Task<Result> DeleteBranch(Guid tenantId, Guid branchId, IMediator mediator) =>
        mediator.Send(new DeleteBranchCommand(tenantId, branchId));

    /// <summary>Status change body.</summary>
    public sealed record ChangeStatusRequest(TenantStatus Status);

    /// <summary>Release ring body (10.7).</summary>
    public sealed record SetReleaseRingRequest(ReleaseRing Ring);

    /// <summary>Create status incident body.</summary>
    public sealed record CreateStatusIncidentRequest(
        string Title,
        StatusIncidentImpact Impact,
        string Components);

    /// <summary>Add branch body.</summary>
    public sealed record AddBranchRequest(string Cnpj, string LegalName, bool IsHeadquarters = false);

    /// <summary>Update branch body.</summary>
    public sealed record UpdateBranchRequest(string LegalName);

    /// <summary>Plan change body.</summary>
    public sealed record ChangePlanRequest(string PlanCode);

    /// <summary>Feature flag body.</summary>
    public sealed record SetFeatureFlagRequest(FeatureFlagState State);

    /// <summary>Billing customer body.</summary>
    public sealed record UpsertBillingCustomerRequest(string Name, string Email, string CpfCnpj);

    /// <summary>Billing payment method body.</summary>
    public sealed record UpsertBillingPaymentMethodRequest(BillingPaymentMethodKind Kind, string? CreditCardToken);

    /// <summary>Create coupon body.</summary>
    public sealed record CreateCouponRequest(
        string Code,
        CouponDiscountType DiscountType,
        decimal Value,
        int? MaxRedemptions,
        DateTimeOffset? ExpiresAt);

    /// <summary>Redeem coupon body.</summary>
    public sealed record RedeemCouponRequest(string Code);

    /// <summary>Create partner API key body.</summary>
    public sealed record CreatePartnerApiKeyRequest(string PartnerName);

    private static IResult ToAdoptionFileResult(Result<ModuleAdoptionFileDto> result)
    {
        if (result.IsFailure)
        {
            return result.ToHttpResult();
        }

        return Results.File(result.Value.Content, result.Value.ContentType, result.Value.FileName);
    }

    private static Task<Result<PlatformSaasMetricsDto>> GetPlatformSaasMetrics(
        int? year,
        int? month,
        IMediator mediator)
    {
        var now = DateTimeOffset.UtcNow;
        var y = year ?? now.Year;
        var m = month ?? now.Month;
        return mediator.Send(new GetPlatformSaasMetricsQuery(y, m));
    }

    private static Task<Result> UpsertAcquisitionSpend(
        [FromBody] UpsertAcquisitionSpendRequest body,
        IMediator mediator) =>
        mediator.Send(new UpsertAcquisitionSpendCommand(body.Year, body.Month, body.Channel, body.Amount, body.Note));

    /// <summary>Acquisition spend body (10.2).</summary>
    public sealed record UpsertAcquisitionSpendRequest(int Year, int Month, string Channel, decimal Amount, string? Note);
}
