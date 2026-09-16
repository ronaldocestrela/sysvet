using Core.Application.AccessProfiles.Dtos;
using Core.Application.Common;
using Core.Domain;
using MediatR;

namespace Core.Application.AccessProfiles.Queries;

/// <summary>
/// Returns paginated access profiles.
/// </summary>
public sealed class ListAccessProfilesQueryHandler : IRequestHandler<ListAccessProfilesQuery, Result<PagedResult<AccessProfileDto>>>
{
    private readonly IAccessProfileRepository _accessProfileRepository;

    public ListAccessProfilesQueryHandler(IAccessProfileRepository accessProfileRepository)
    {
        _accessProfileRepository = accessProfileRepository;
    }

    /// <inheritdoc />
    public async Task<Result<PagedResult<AccessProfileDto>>> Handle(ListAccessProfilesQuery request, CancellationToken cancellationToken)
    {
        var page = await _accessProfileRepository.SearchAsync(
            request.Page,
            request.PageSize,
            request.NameFilter,
            cancellationToken);

        var items = page.Items.Select(AccessProfileMappings.ToDto).ToList();
        return Result.Success(new PagedResult<AccessProfileDto>(items, request.Page, request.PageSize, page.TotalCount));
    }
}
