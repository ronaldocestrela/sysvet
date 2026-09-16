using Core.Application.Common;
using Core.Application.Common.Interfaces;
using Core.Application.Users.Dtos;
using Core.Domain;
using MediatR;

namespace Core.Application.Users.Queries;

/// <summary>
/// Lists staff users in the authenticated tenant.
/// </summary>
public sealed class ListUsersQueryHandler : IRequestHandler<ListUsersQuery, Result<PagedResult<StaffUserDto>>>
{
    private readonly IIdentityService _identityService;
    private readonly ITenantContext _tenantContext;

    public ListUsersQueryHandler(IIdentityService identityService, ITenantContext tenantContext)
    {
        _identityService = identityService;
        _tenantContext = tenantContext;
    }

    /// <inheritdoc />
    public async Task<Result<PagedResult<StaffUserDto>>> Handle(ListUsersQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<PagedResult<StaffUserDto>>(ErrorCodes.Authorization.Forbidden);
        }

        return await _identityService.ListByTenantAsync(
            tenantId,
            request.Page,
            request.PageSize,
            request.Search,
            cancellationToken);
    }
}
