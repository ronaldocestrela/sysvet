using Core.Application.Authorization;
using Core.Domain;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Core.Domain.Entitlements;
using Platform.Application.Subscriptions;
using Platform.Application.Tenants.Commands;
using Platform.Application.Tenants.Dtos;
using Platform.Application.Tenants.Queries;
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

        return builder;
    }

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

    private static Task<Result<OnboardTenantResultDto>> OnboardTenant([FromBody] OnboardTenantCommand command, IMediator mediator) =>
        mediator.Send(command);

    private static Task<Result<IReadOnlyList<TenantSummaryDto>>> ListTenants([FromQuery] bool activeOnly, IMediator mediator) =>
        mediator.Send(new ListTenantsQuery(activeOnly));

    private static Task<Result<TenantDetailDto>> GetTenant(Guid tenantId, IMediator mediator) =>
        mediator.Send(new GetTenantQuery(tenantId));

    private static Task<Result> ChangeStatus(Guid tenantId, [FromBody] ChangeStatusRequest body, IMediator mediator) =>
        mediator.Send(new ChangeTenantStatusCommand(tenantId, body.Status));

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

    /// <summary>Add branch body.</summary>
    public sealed record AddBranchRequest(string Cnpj, string LegalName, bool IsHeadquarters = false);

    /// <summary>Update branch body.</summary>
    public sealed record UpdateBranchRequest(string LegalName);

    /// <summary>Plan change body.</summary>
    public sealed record ChangePlanRequest(string PlanCode);

    /// <summary>Feature flag body.</summary>
    public sealed record SetFeatureFlagRequest(FeatureFlagState State);
}
