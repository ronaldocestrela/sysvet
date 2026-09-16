using Core.Domain;
using MediatR;
using Veterinary.Domain.Repositories;

namespace Veterinary.Application.Hospitalizations.Commands;

public class ExecutePrescriptionCommandHandler : IRequestHandler<ExecutePrescriptionCommand, Result<Guid>>
{
    private readonly IHospitalizationRepository _hospitalizationRepository;
    private readonly IPrescriptionExecutionRepository _prescriptionRepository;

    public ExecutePrescriptionCommandHandler(
        IHospitalizationRepository hospitalizationRepository,
        IPrescriptionExecutionRepository prescriptionRepository)
    {
        _hospitalizationRepository = hospitalizationRepository;
        _prescriptionRepository = prescriptionRepository;
    }

    public async Task<Result<Guid>> Handle(ExecutePrescriptionCommand request, CancellationToken cancellationToken)
    {
        var hosp = await _hospitalizationRepository.GetByIdAsync(request.HospitalizationId, cancellationToken);

        if (hosp == null)
        {
            return Result.Failure<Guid>(Veterinary.Domain.ErrorCodes.Hospitalization.NotFound);
        }

        var execResult = hosp.ExecutePrescription(request.MedicationName, request.Dose, request.Notes, request.ExecutedBy);

        if (execResult.IsFailure)
        {
            return Result.Failure<Guid>(execResult.Error);
        }

        var execution = hosp.PrescriptionExecutions.Last();

        await _prescriptionRepository.AddAsync(execution, cancellationToken);

        return Result.Success(execution.Id);
    }
}
