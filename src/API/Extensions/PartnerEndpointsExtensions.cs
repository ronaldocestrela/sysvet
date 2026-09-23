using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Platform.Application.Abstractions;
using Platform.Application.Health;
namespace API.Extensions;

/// <summary>Partner integration routes authenticated via API key (9.7).</summary>
public static class PartnerEndpointsExtensions
{
    /// <summary>Maps partner routes under <c>/api/v1/partner</c>.</summary>
    public static IEndpointRouteBuilder MapPartnerEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/v1/partner")
            .WithTags("Platform", "Partner");

        group.MapGet("/health", GetPartnerHealth);

        return builder;
    }

    private static async Task<IResult> GetPartnerHealth(IPartnerRequestContext partnerContext, IMediator mediator)
    {
        if (partnerContext.AuthenticatedTenantId is not { } tenantId || tenantId == Guid.Empty)
        {
            return Results.Unauthorized();
        }

        var result = await mediator.Send(new GetTenantHealthQuery(tenantId));
        return result.ToHttpResult();
    }
}
