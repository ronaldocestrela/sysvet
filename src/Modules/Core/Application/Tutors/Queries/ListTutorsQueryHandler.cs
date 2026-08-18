using Core.Domain;
using MediatR;

namespace Core.Application.Tutors.Queries;

public class ListTutorsQueryHandler : IRequestHandler<ListTutorsQuery, Result<IEnumerable<TutorDto>>>
{
    private readonly ITutorRepository _tutorRepository;

    public ListTutorsQueryHandler(ITutorRepository tutorRepository)
    {
        _tutorRepository = tutorRepository;
    }

    public async Task<Result<IEnumerable<TutorDto>>> Handle(ListTutorsQuery request, CancellationToken cancellationToken)
    {
        var allTutors = await _tutorRepository.GetAllAsync(cancellationToken);

        var filtered = allTutors.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(request.NameFilter))
        {
            filtered = filtered.Where(t => t.Name.Contains(request.NameFilter, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(request.CpfFilter))
        {
            filtered = filtered.Where(t => t.Cpf.Number.Contains(request.CpfFilter));
        }

        var paged = filtered
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new TutorDto
            {
                Id = t.Id,
                Name = t.Name,
                Email = t.Email.Address,
                Cpf = t.Cpf.Number,
                Phone = t.Phone.Number
            })
            .ToList();

        return Result.Success<IEnumerable<TutorDto>>(paged);
    }
}
