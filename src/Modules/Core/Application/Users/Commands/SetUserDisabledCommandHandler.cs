using Core.Application.Authorization;
using Core.Application.Common.Interfaces;
using Core.Domain;
using MediatR;

namespace Core.Application.Users.Commands;

/// <summary>
/// Disables or enables users while protecting the last admin.
/// </summary>
public sealed class SetUserDisabledCommandHandler : IRequestHandler<SetUserDisabledCommand, Result>
{
    private readonly IIdentityService _identityService;
    private readonly ITenantContext _tenantContext;

    public SetUserDisabledCommandHandler(IIdentityService identityService, ITenantContext tenantContext)
    {
        _identityService = identityService;
        _tenantContext = tenantContext;
    }

    /// <inheritdoc />
    public async Task<Result> Handle(SetUserDisabledCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;
        if (tenantId == Guid.Empty)
        {
            return Result.Failure(ErrorCodes.Authorization.Forbidden);
        }

        if (request.Disabled)
        {
            var userResult = await _identityService.GetByIdAsync(request.UserId, tenantId, cancellationToken);
            if (userResult.IsFailure)
            {
                return Result.Failure(userResult.Error);
            }

            if (userResult.Value.Roles.Contains(ApplicationRoles.Admin))
            {
                var adminCount = await _identityService.CountAdminsInTenantAsync(tenantId, cancellationToken);
                if (adminCount <= 1)
                {
                    return Result.Failure(ErrorCodes.UserAccount.LastAdmin);
                }
            }
        }

        return await _identityService.SetDisabledAsync(request.UserId, tenantId, request.Disabled, cancellationToken);
    }
}
