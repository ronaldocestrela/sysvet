using Core.Domain;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Repositories;
using IUnitOfWork = Veterinary.Domain.Repositories.IUnitOfWork;

namespace Veterinary.Application.Hospitalizations.Commands;

public class AdmitPetCommandHandler : IRequestHandler<AdmitPetCommand, Result<Guid>>
{
    private readonly IHospitalizationRepository _hospitalizationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AdmitPetCommandHandler(IHospitalizationRepository hospitalizationRepository, IUnitOfWork unitOfWork)
    {
        _hospitalizationRepository = hospitalizationRepository;
        _unitOfWork = unitOfWork;
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
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(hosp.Id);
    }
}
