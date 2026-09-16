using Core.Domain;
using MediatR;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Repositories;

namespace Veterinary.Application.Vaccines.Commands;

public class RegisterVaccineDoseCommandHandler : IRequestHandler<RegisterVaccineDoseCommand, Result<Guid>>
{
    private readonly IVaccineDoseRepository _vaccineDoseRepository;

    public RegisterVaccineDoseCommandHandler(IVaccineDoseRepository vaccineDoseRepository)
    {
        _vaccineDoseRepository = vaccineDoseRepository;
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

        return Result.Success(vaccineDose.Id);
    }
}
