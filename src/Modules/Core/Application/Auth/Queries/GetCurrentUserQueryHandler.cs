using Core.Application.Auth.Dtos;
using Core.Application.Common.Interfaces;
using Core.Domain;
using MediatR;

namespace Core.Application.Auth.Queries;

/// <summary>
/// Maps <see cref="ICurrentUser"/> claims to a profile DTO.
/// </summary>
public sealed class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, Result<CurrentUserDto>>
{
    private readonly ICurrentUser _currentUser;
    private readonly ITenantContext _tenantContext;

    public GetCurrentUserQueryHandler(ICurrentUser currentUser, ITenantContext tenantContext)
    {
        _currentUser = currentUser;
        _tenantContext = tenantContext;
    }

    /// <inheritdoc />
    public Task<Result<CurrentUserDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || string.IsNullOrEmpty(_currentUser.UserId))
        {
            return Task.FromResult(Result.Failure<CurrentUserDto>(ErrorCodes.Authorization.Unauthorized));
        }

        var tenantId = _currentUser.TenantId != Guid.Empty ? _currentUser.TenantId : _tenantContext.TenantId;

        var dto = new CurrentUserDto(
            _currentUser.UserId,
            _currentUser.Email ?? string.Empty,
            tenantId,
            _currentUser.Roles);

        return Task.FromResult(Result.Success(dto));
    }
}
