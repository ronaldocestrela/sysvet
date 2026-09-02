using Core.Domain;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Repositories;
using IUnitOfWork = Veterinary.Domain.Repositories.IUnitOfWork;

namespace Veterinary.Application.Vaccines.Commands;

public class RegisterVaccineDoseCommandHandler : IRequestHandler<RegisterVaccineDoseCommand, Result<Guid>>
{
    private readonly IVaccineDoseRepository _vaccineDoseRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterVaccineDoseCommandHandler(
        IVaccineDoseRepository vaccineDoseRepository,
        IUnitOfWork unitOfWork)
    {
        _vaccineDoseRepository = vaccineDoseRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(RegisterVaccineDoseCommand request, CancellationToken cancellationToken)
    {
        var vaccineResult = VaccineDose.Create(
            Guid.NewGuid(),
            request.PetId,
            request.Name,
            request.BatchNumber,
            request.AppliedAt,
            request.NextDueDate);

        if (vaccineResult.IsFailure)
        {
            return Result.Failure<Guid>(vaccineResult.Error);
        }

        var vaccineDose = vaccineResult.Value;

        await _vaccineDoseRepository.AddAsync(vaccineDose, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(vaccineDose.Id);
    }
}
