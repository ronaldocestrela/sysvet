using Core.Domain;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Veterinary.Domain.Repositories;
using IUnitOfWork = Veterinary.Domain.Repositories.IUnitOfWork;

namespace Veterinary.Application.Hospitalizations.Commands;

public class ExecutePrescriptionCommandHandler : IRequestHandler<ExecutePrescriptionCommand, Result<Guid>>
{
    private readonly IHospitalizationRepository _hospitalizationRepository;
    private readonly IPrescriptionExecutionRepository _prescriptionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ExecutePrescriptionCommandHandler(
        IHospitalizationRepository hospitalizationRepository,
        IPrescriptionExecutionRepository prescriptionRepository,
        IUnitOfWork unitOfWork)
    {
        _hospitalizationRepository = hospitalizationRepository;
        _prescriptionRepository = prescriptionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(ExecutePrescriptionCommand request, CancellationToken cancellationToken)
    {
        var hosp = await _hospitalizationRepository.GetByIdAsync(request.HospitalizationId, cancellationToken);

        if (hosp == null)
        {
            return Result.Failure<Guid>(new Error("Hospitalization.NotFound", "The specified hospitalization was not found."));
        }

        var execResult = hosp.ExecutePrescription(request.MedicationName, request.Dose, request.Notes, request.ExecutedBy);

        if (execResult.IsFailure)
        {
            return Result.Failure<Guid>(execResult.Error);
        }

        var execution = hosp.PrescriptionExecutions.Last();

        await _prescriptionRepository.AddAsync(execution, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(execution.Id);
    }
}
