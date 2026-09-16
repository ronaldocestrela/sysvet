using Core.Application.Common;
using Core.Domain;
using MediatR;

namespace Core.Application.Tutors.Queries;

/// <summary>
/// Returns a paginated list of tutors with optional name and CPF filters.
/// </summary>
public class ListTutorsQueryHandler : IRequestHandler<ListTutorsQuery, Result<PagedResult<TutorDto>>>
{
    private readonly ITutorRepository _tutorRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="ListTutorsQueryHandler"/> class.
    /// </summary>
    public ListTutorsQueryHandler(ITutorRepository tutorRepository)
    {
        _tutorRepository = tutorRepository;
    }

    /// <inheritdoc />
    public async Task<Result<PagedResult<TutorDto>>> Handle(ListTutorsQuery request, CancellationToken cancellationToken)
    {
        var page = await _tutorRepository.SearchAsync(
            request.Page,
            request.PageSize,
            request.NameFilter,
            request.CpfFilter,
            cancellationToken);

        var items = page.Items.Select(TutorMappings.ToDto).ToList();
        var paged = new PagedResult<TutorDto>(items, request.Page, request.PageSize, page.TotalCount);

        return Result.Success(paged);
    }
}
