using Core.Domain;
using MediatR;

namespace Core.Application.Tutors.Queries;

public class GetTutorByIdQueryHandler : IRequestHandler<GetTutorByIdQuery, Result<TutorDto>>
{
    private readonly ITutorRepository _tutorRepository;

    public GetTutorByIdQueryHandler(ITutorRepository tutorRepository)
    {
        _tutorRepository = tutorRepository;
    }

    public async Task<Result<TutorDto>> Handle(GetTutorByIdQuery request, CancellationToken cancellationToken)
    {
        var tutor = await _tutorRepository.GetByIdAsync(request.Id, cancellationToken);

        if (tutor == null)
        {
            return Result.Failure<TutorDto>(new Error("Tutor.NotFound", $"O Tutor com ID '{request.Id}' não foi encontrado."));
        }

        var dto = new TutorDto
        {
            Id = tutor.Id,
            Name = tutor.Name,
            Email = tutor.Email.Address,
            Cpf = tutor.Cpf.Number,
            Phone = tutor.Phone.Number
        };

        return Result.Success(dto);
    }
}
