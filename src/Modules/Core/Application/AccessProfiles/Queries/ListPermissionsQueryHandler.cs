using Core.Domain.Authorization;
using Core.Domain;
using MediatR;

namespace Core.Application.AccessProfiles.Queries;

/// <summary>
/// Exposes all catalog permission codes.
/// </summary>
public sealed class ListPermissionsQueryHandler : IRequestHandler<ListPermissionsQuery, Result<IReadOnlyList<string>>>
{
    /// <inheritdoc />
    public Task<Result<IReadOnlyList<string>>> Handle(ListPermissionsQuery request, CancellationToken cancellationToken) =>
        Task.FromResult(Result.Success(Permissions.All));
}
