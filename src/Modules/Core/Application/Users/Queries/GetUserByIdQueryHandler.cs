using Core.Application.Common.Interfaces;
using Core.Application.Users.Dtos;
using Core.Domain;
using MediatR;

namespace Core.Application.Users.Queries;

/// <summary>
/// Returns a single staff user for admin APIs.
/// </summary>
public sealed class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<StaffUserDto>>
{
    private readonly IIdentityService _identityService;
    private readonly ITenantContext _tenantContext;

    public GetUserByIdQueryHandler(IIdentityService identityService, ITenantContext tenantContext)
    {
        _identityService = identityService;
        _tenantContext = tenantContext;
    }

    /// <inheritdoc />
    public async Task<Result<StaffUserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<StaffUserDto>(ErrorCodes.Authorization.Forbidden);
        }

        return await _identityService.GetByIdAsync(request.UserId, tenantId, cancellationToken);
    }
}
