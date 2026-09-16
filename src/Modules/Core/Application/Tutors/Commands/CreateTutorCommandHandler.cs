using Core.Domain;
using Core.Domain.Entities;
using Core.Domain.ValueObjects;
using MediatR;

namespace Core.Application.Tutors.Commands;

/// <summary>
/// Validates uniqueness and persists a new tutor aggregate.
/// </summary>
public class CreateTutorCommandHandler : IRequestHandler<CreateTutorCommand, Result<Guid>>
{
    private readonly ITutorRepository _tutorRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateTutorCommandHandler"/> class.
    /// </summary>
    public CreateTutorCommandHandler(ITutorRepository tutorRepository)
    {
        _tutorRepository = tutorRepository;
    }

    /// <inheritdoc />
    public async Task<Result<Guid>> Handle(CreateTutorCommand request, CancellationToken cancellationToken)
    {
        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure) return Result.Failure<Guid>(emailResult.Error);

        var cpfResult = Cpf.Create(request.Cpf);
        if (cpfResult.IsFailure) return Result.Failure<Guid>(cpfResult.Error);

        var phoneResult = Phone.Create(request.Phone);
        if (phoneResult.IsFailure) return Result.Failure<Guid>(phoneResult.Error);

        var existingCpf = await _tutorRepository.GetByCpfAsync(cpfResult.Value.Number, cancellationToken);
        if (existingCpf is not null)
        {
            return Result.Failure<Guid>(ErrorCodes.Tutor.DuplicateCpf);
        }

        var existingEmail = await _tutorRepository.GetByEmailAsync(emailResult.Value.Address, cancellationToken);
        if (existingEmail is not null)
        {
            return Result.Failure<Guid>(ErrorCodes.Tutor.DuplicateEmail);
        }

        var tutorResult = Tutor.Create(request.Name, emailResult.Value, cpfResult.Value, phoneResult.Value, request.Id);
        if (tutorResult.IsFailure) return Result.Failure<Guid>(tutorResult.Error);

        _tutorRepository.Add(tutorResult.Value);

        return Result.Success(tutorResult.Value.Id);
    }
}
