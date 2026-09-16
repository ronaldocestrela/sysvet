using Core.Domain;
using MediatR;
using Veterinary.Domain.Repositories;

namespace Veterinary.Application.Hospitalizations.Commands;

public class DischargePetCommandHandler : IRequestHandler<DischargePetCommand, Result<bool>>
{
    private readonly IHospitalizationRepository _hospitalizationRepository;

    public DischargePetCommandHandler(IHospitalizationRepository hospitalizationRepository)
    {
        _hospitalizationRepository = hospitalizationRepository;
    }

    public async Task<Result<bool>> Handle(DischargePetCommand request, CancellationToken cancellationToken)
    {
        var hosp = await _hospitalizationRepository.GetByIdAsync(request.HospitalizationId, cancellationToken);

        if (hosp == null)
        {
            return Result.Failure<bool>(Veterinary.Domain.ErrorCodes.Hospitalization.NotFound);
        }

        var dischargeResult = hosp.Discharge();

        if (dischargeResult.IsFailure)
        {
            return dischargeResult;
        }

        _hospitalizationRepository.Update(hosp);

        return Result.Success(true);
    }
}
