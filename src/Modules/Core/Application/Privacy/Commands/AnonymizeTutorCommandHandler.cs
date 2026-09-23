using Core.Application.Privacy;
using Core.Domain;
using MediatR;

namespace Core.Application.Privacy.Commands;

/// <summary>Anonymizes the tutor aggregate and module-local PII copies.</summary>
public sealed class AnonymizeTutorCommandHandler : IRequestHandler<AnonymizeTutorCommand, Result>
{
    private readonly ITutorRepository _tutorRepository;
    private readonly IEnumerable<IPersonalDataErasureContributor> _erasureContributors;

    /// <summary>Initializes handler dependencies.</summary>
    public AnonymizeTutorCommandHandler(
        ITutorRepository tutorRepository,
        IEnumerable<IPersonalDataErasureContributor> erasureContributors)
    {
        _tutorRepository = tutorRepository;
        _erasureContributors = erasureContributors;
    }

    /// <inheritdoc />
    public async Task<Result> Handle(AnonymizeTutorCommand request, CancellationToken cancellationToken)
    {
        var tutor = await _tutorRepository.GetByIdAsync(request.TutorId, cancellationToken);
        if (tutor is null)
        {
            return Result.Failure(ErrorCodes.Tutor.NotFound);
        }

        if (tutor.IsAnonymized)
        {
            return Result.Success();
        }

        var context = new PersonalDataErasureContext(
            tutor.Id,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            tutor.Cpf.Number,
            tutor.Email.Address);

        var anonymizeResult = tutor.Anonymize();
        if (anonymizeResult.IsFailure)
        {
            return anonymizeResult;
        }

        context = context with
        {
            TombstoneName = tutor.Name,
            TombstoneEmail = tutor.Email.Address,
            TombstoneCpf = tutor.Cpf.Number,
            TombstonePhone = tutor.Phone.Number
        };

        foreach (var contributor in _erasureContributors)
        {
            var eraseResult = await contributor.EraseForTutorAsync(context, cancellationToken);
            if (eraseResult.IsFailure)
            {
                return eraseResult;
            }
        }

        _tutorRepository.Update(tutor);
        return Result.Success();
    }
}
