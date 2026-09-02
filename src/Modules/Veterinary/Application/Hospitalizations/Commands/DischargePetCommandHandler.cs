using Core.Domain;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Veterinary.Domain.Repositories;
using IUnitOfWork = Veterinary.Domain.Repositories.IUnitOfWork;

namespace Veterinary.Application.Hospitalizations.Commands;

public class DischargePetCommandHandler : IRequestHandler<DischargePetCommand, Result<bool>>
{
    private readonly IHospitalizationRepository _hospitalizationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DischargePetCommandHandler(IHospitalizationRepository hospitalizationRepository, IUnitOfWork unitOfWork)
    {
        _hospitalizationRepository = hospitalizationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(DischargePetCommand request, CancellationToken cancellationToken)
    {
        var hosp = await _hospitalizationRepository.GetByIdAsync(request.HospitalizationId, cancellationToken);

        if (hosp == null)
        {
            return Result.Failure<bool>(new Error("Hospitalization.NotFound", "The specified hospitalization was not found."));
        }

        var dischargeResult = hosp.Discharge();

        if (dischargeResult.IsFailure)
        {
            return dischargeResult;
        }

        _hospitalizationRepository.Update(hosp);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
