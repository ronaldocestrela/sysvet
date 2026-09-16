using Core.Application.Common.Interfaces;
using Core.Domain;
using MediatR;

namespace Core.Application.Users.Commands;

/// <summary>
/// Applies profile and display name changes to a tenant user.
/// </summary>
public sealed class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result>
{
    private readonly IIdentityService _identityService;
    private readonly ITenantContext _tenantContext;

    public UpdateUserCommandHandler(IIdentityService identityService, ITenantContext tenantContext)
    {
        _identityService = identityService;
        _tenantContext = tenantContext;
    }

    /// <inheritdoc />
    public async Task<Result> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;
        if (tenantId == Guid.Empty)
        {
            return Result.Failure(ErrorCodes.Authorization.Forbidden);
        }

        return await _identityService.UpdateProfileAndNameAsync(
            request.UserId,
            tenantId,
            request.AccessProfileId,
            request.DisplayName,
            cancellationToken);
    }
}
