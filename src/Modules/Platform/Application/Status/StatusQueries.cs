using Core.Application.Messaging;

namespace Platform.Application.Status;

/// <summary>Lists recent incidents for Super Admin.</summary>
public sealed record ListStatusIncidentsQuery(int Take = 50) : IQuery<IReadOnlyList<StatusIncidentDto>>;

/// <summary>Builds anonymous public status payload.</summary>
public sealed record GetPublicStatusQuery : IQuery<PublicStatusDto>;
