using Core.Application.IntegrationEvents;
using Core.Domain;
using MediatR;

namespace Core.Application.Tutors.Integration;

/// <summary>Loads tutor data for fiscal recipient block.</summary>
public sealed class GetTutorFiscalDestRequestHandler
    : IRequestHandler<GetTutorFiscalDestRequest, Result<TutorFiscalDest?>>
{
    private readonly ITutorRepository _tutorRepository;

    public GetTutorFiscalDestRequestHandler(ITutorRepository tutorRepository)
    {
        _tutorRepository = tutorRepository;
    }

    public async Task<Result<TutorFiscalDest?>> Handle(
        GetTutorFiscalDestRequest request,
        CancellationToken cancellationToken)
    {
        var tutor = await _tutorRepository.GetByIdAsync(request.TutorId, cancellationToken);
        if (tutor is null || tutor.IsDeleted)
        {
            return Result.Success<TutorFiscalDest?>(null);
        }

        return Result.Success<TutorFiscalDest?>(new TutorFiscalDest
        {
            Name = tutor.Name,
            Cpf = tutor.Cpf.Number,
            Email = tutor.Email.Address,
            Phone = tutor.Phone.Number,
            Street = tutor.Address?.Street,
            Number = tutor.Address?.Number,
            Complement = tutor.Address?.Complement,
            District = tutor.Address?.District,
            City = tutor.Address?.City,
            State = tutor.Address?.State,
            PostalCode = tutor.Address?.PostalCode,
            IbgeCityCode = tutor.Address?.IbgeCityCode
        });
    }
}
