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
            return Result.Failure<TutorDto>(ErrorCodes.Tutor.NotFound);
        }

        return Result.Success(TutorMappings.ToDto(tutor));
    }
}
