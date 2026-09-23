using Core.Application.Common.Interfaces;
using Core.Application.IntegrationEvents;
using Core.Domain;
using MediatR;

namespace Core.Application.Integration;

/// <summary>Resolves staff display names for intelligence productivity reports.</summary>
public sealed class GetStaffDisplayNamesRequestHandler
    : IRequestHandler<GetStaffDisplayNamesRequest, Result<IReadOnlyDictionary<Guid, string>>>
{
    private readonly IIdentityService _identityService;
    private readonly ICurrentUser _currentUser;

    /// <summary>Creates the handler.</summary>
    public GetStaffDisplayNamesRequestHandler(IIdentityService identityService, ICurrentUser currentUser)
    {
        _identityService = identityService;
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyDictionary<Guid, string>>> Handle(
        GetStaffDisplayNamesRequest request,
        CancellationToken cancellationToken)
    {
        if (request.UserIds.Count == 0)
        {
            return Result.Success<IReadOnlyDictionary<Guid, string>>(new Dictionary<Guid, string>());
        }

        var tenantId = _currentUser.TenantId;
        var map = new Dictionary<Guid, string>();
        foreach (var id in request.UserIds.Where(i => i != Guid.Empty).Distinct())
        {
            var userResult = await _identityService.GetByIdAsync(id.ToString("D"), tenantId, cancellationToken);
            if (userResult.IsSuccess)
            {
                var display = userResult.Value.DisplayName ?? userResult.Value.Email;
                map[id] = display;
            }
        }

        return Result.Success<IReadOnlyDictionary<Guid, string>>(map);
    }
}
