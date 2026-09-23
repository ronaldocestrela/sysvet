using Core.Application.Authorization;
using Core.Domain;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Platform.Application.Billing;

namespace API.Extensions;

/// <summary>Clinic admin SaaS billing routes (9.5).</summary>
public static class PlatformClinicBillingEndpointExtensions
{
    /// <summary>Maps <c>/api/v1/billing</c> for locked-tenant payment.</summary>
    public static IEndpointRouteBuilder MapClinicBillingEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/v1/billing")
            .WithTags("Billing")
            .RequireAuthorization(AuthorizationPolicies.Admin);

        group.MapGet("/standing", GetStanding);
        group.MapPost("/pay", PayOutstanding);

        return builder;
    }

    private static async Task<Result<ClinicBillingStandingDto>> GetStanding(
        ITenantContext tenantContext,
        IMediator mediator) =>
        await mediator.Send(new GetClinicBillingStandingQuery(tenantContext.TenantId));

    private static async Task<Result<PayClinicBillingResultDto>> PayOutstanding(
        ITenantContext tenantContext,
        IMediator mediator) =>
        await mediator.Send(new PayClinicBillingCommand(tenantContext.TenantId, DateTimeOffset.UtcNow));
}
