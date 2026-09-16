using Core.Application.Common;
using Core.Domain;
using MediatR;

namespace Core.Application.Pets.Queries;

/// <summary>
/// Returns a paginated list of pets with optional tutor and name filters.
/// </summary>
public class ListPetsQueryHandler : IRequestHandler<ListPetsQuery, Result<PagedResult<PetDto>>>
{
    private readonly IPetRepository _petRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="ListPetsQueryHandler"/> class.
    /// </summary>
    public ListPetsQueryHandler(IPetRepository petRepository)
    {
        _petRepository = petRepository;
    }

    /// <inheritdoc />
    public async Task<Result<PagedResult<PetDto>>> Handle(ListPetsQuery request, CancellationToken cancellationToken)
    {
        var page = await _petRepository.SearchAsync(
            request.Page,
            request.PageSize,
            request.TutorId,
            request.NameFilter,
            cancellationToken);

        var items = page.Items.Select(PetMappings.ToDto).ToList();
        var paged = new PagedResult<PetDto>(items, request.Page, request.PageSize, page.TotalCount);

        return Result.Success(paged);
    }
}
