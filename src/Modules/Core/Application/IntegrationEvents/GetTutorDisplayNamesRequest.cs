using Core.Domain;
using MediatR;

namespace Core.Application.IntegrationEvents;

/// <summary>Resolves tutor display names for intelligence reports (Core CRM).</summary>
public sealed class GetTutorDisplayNamesRequest : IRequest<Result<IReadOnlyDictionary<Guid, string>>>
{
    /// <summary>Tutor ids to resolve.</summary>
    public IReadOnlyList<Guid> TutorIds { get; }

    /// <summary>Creates the request.</summary>
    public GetTutorDisplayNamesRequest(IReadOnlyList<Guid> tutorIds) => TutorIds = tutorIds;
}
