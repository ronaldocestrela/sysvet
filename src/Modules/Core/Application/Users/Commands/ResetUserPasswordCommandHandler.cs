using Core.Application.Common.Interfaces;
using Core.Domain;
using MediatR;

namespace Core.Application.Users.Commands;

/// <summary>
/// Delegates password reset to Identity.
/// </summary>
public sealed class ResetUserPasswordCommandHandler : IRequestHandler<ResetUserPasswordCommand, Result>
{
    private readonly IIdentityService _identityService;
    private readonly ITenantContext _tenantContext;

    public ResetUserPasswordCommandHandler(IIdentityService identityService, ITenantContext tenantContext)
    {
        _identityService = identityService;
        _tenantContext = tenantContext;
    }

    /// <inheritdoc />
    public async Task<Result> Handle(ResetUserPasswordCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;
        if (tenantId == Guid.Empty)
        {
            return Result.Failure(ErrorCodes.Authorization.Forbidden);
        }

        return await _identityService.ResetPasswordAsync(request.UserId, tenantId, request.NewPassword, cancellationToken);
    }
}
