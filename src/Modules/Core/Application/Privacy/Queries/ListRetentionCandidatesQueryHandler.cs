using Core.Domain;
using MediatR;

namespace Core.Application.Privacy.Queries;

/// <summary>Evaluates retention policy against soft-deleted tutors.</summary>
public sealed class ListRetentionCandidatesQueryHandler
    : IRequestHandler<ListRetentionCandidatesQuery, Result<IReadOnlyList<RetentionCandidateDto>>>
{
    private readonly ITutorRepository _tutorRepository;

    /// <summary>Initializes handler dependencies.</summary>
    public ListRetentionCandidatesQueryHandler(ITutorRepository tutorRepository)
    {
        _tutorRepository = tutorRepository;
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<RetentionCandidateDto>>> Handle(
        ListRetentionCandidatesQuery request,
        CancellationToken cancellationToken)
    {
        var asOf = request.AsOfUtc ?? DateTimeOffset.UtcNow;
        var candidates = await _tutorRepository.ListRetentionCandidatesAsync(asOf, cancellationToken);
        var dtos = candidates
            .Select(t => new RetentionCandidateDto { TutorId = t.Id, DeletedAt = t.DeletedAt })
            .ToList();

        return Result.Success<IReadOnlyList<RetentionCandidateDto>>(dtos);
    }
}
