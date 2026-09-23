using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;
using Core.Domain.Authorization;

namespace Core.Application.Privacy.Queries;

/// <summary>Lists soft-deleted tutors eligible for scheduled anonymization.</summary>
[AuthorizeRequest(AuthorizationPolicies.Admin, Permissions.PrivacyErase)]
public sealed record ListRetentionCandidatesQuery(DateTimeOffset? AsOfUtc = null) : IQuery<IReadOnlyList<RetentionCandidateDto>>;

/// <summary>Retention candidate summary.</summary>
public sealed class RetentionCandidateDto
{
    /// <summary>Tutor id.</summary>
    public Guid TutorId { get; init; }

    /// <summary>Soft-delete timestamp.</summary>
    public DateTimeOffset? DeletedAt { get; init; }
}
