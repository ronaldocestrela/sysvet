using Core.Application.Auth.Dtos;
using Core.Application.Common.Interfaces;
using Core.Domain;
using Core.Domain.Authorization;
using MediatR;

namespace Core.Application.Auth.Queries;

/// <summary>
/// Maps <see cref="ICurrentUser"/> claims to a profile DTO including permissions and menus.
/// </summary>
public sealed class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, Result<CurrentUserDto>>
{
    private readonly ICurrentUser _currentUser;
    private readonly ITenantContext _tenantContext;
    private readonly IIdentityService _identityService;
    private readonly IPermissionChecker _permissionChecker;

    public GetCurrentUserQueryHandler(
        ICurrentUser currentUser,
        ITenantContext tenantContext,
        IIdentityService identityService,
        IPermissionChecker permissionChecker)
    {
        _currentUser = currentUser;
        _tenantContext = tenantContext;
        _identityService = identityService;
        _permissionChecker = permissionChecker;
    }

    /// <inheritdoc />
    public async Task<Result<CurrentUserDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || string.IsNullOrEmpty(_currentUser.UserId))
        {
            return Result.Failure<CurrentUserDto>(ErrorCodes.Authorization.Unauthorized);
        }

        var tenantId = _currentUser.TenantId != Guid.Empty ? _currentUser.TenantId : _tenantContext.TenantId;

        var staff = await _identityService.GetByIdAsync(_currentUser.UserId, tenantId, cancellationToken);
        if (staff.IsFailure)
        {
            return Result.Failure<CurrentUserDto>(staff.Error);
        }

        var permissions = await _permissionChecker.GetGrantedPermissionsAsync(cancellationToken);
        var menus = MenuCatalog.ResolveMenus(permissions);

        var dto = new CurrentUserDto(
            _currentUser.UserId,
            _currentUser.Email ?? string.Empty,
            tenantId,
            _currentUser.Roles,
            staff.Value.AccessProfileId,
            staff.Value.AccessProfileName,
            permissions,
            menus);

        return Result.Success(dto);
    }
}
