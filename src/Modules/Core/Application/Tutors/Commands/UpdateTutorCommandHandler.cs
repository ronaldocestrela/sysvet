using Core.Domain;
using Core.Domain.ValueObjects;
using MediatR;

namespace Core.Application.Tutors.Commands;

/// <summary>
/// Updates mutable tutor fields; audit is captured on save via <see cref="Core.Infrastructure.Persistence.CoreDbContext"/>.
/// </summary>
public class UpdateTutorCommandHandler : IRequestHandler<UpdateTutorCommand, Result>
{
    private readonly ITutorRepository _tutorRepository;

    public UpdateTutorCommandHandler(ITutorRepository tutorRepository)
    {
        _tutorRepository = tutorRepository;
    }

    /// <inheritdoc />
    public async Task<Result> Handle(UpdateTutorCommand request, CancellationToken cancellationToken)
    {
        var tutor = await _tutorRepository.GetByIdAsync(request.Id, cancellationToken);
        if (tutor == null)
        {
            return Result.Failure(ErrorCodes.Tutor.NotFound);
        }

        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure) return emailResult;

        var phoneResult = Phone.Create(request.Phone);
        if (phoneResult.IsFailure) return phoneResult;

        var updateResult = tutor.Update(request.Name, emailResult.Value, phoneResult.Value);
        if (updateResult.IsFailure) return updateResult;

        _tutorRepository.Update(tutor);

        return Result.Success();
    }
}
