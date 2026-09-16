using Core.Domain;
using MediatR;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Repositories;

namespace Veterinary.Application.Hospitalizations.Commands;

public class AdmitPetCommandHandler : IRequestHandler<AdmitPetCommand, Result<Guid>>
{
    private readonly IHospitalizationRepository _hospitalizationRepository;

    public AdmitPetCommandHandler(IHospitalizationRepository hospitalizationRepository)
    {
        _hospitalizationRepository = hospitalizationRepository;
    }

    public async Task<Result<Guid>> Handle(AdmitPetCommand request, CancellationToken cancellationToken)
    {
        var admitResult = Hospitalization.Admit(Guid.NewGuid(), request.PetId, request.VeterinarianId, request.Reason);

        if (admitResult.IsFailure)
        {
            return Result.Failure<Guid>(admitResult.Error);
        }

        var hosp = admitResult.Value;
        await _hospitalizationRepository.AddAsync(hosp, cancellationToken);

        return Result.Success(hosp.Id);
    }
}
