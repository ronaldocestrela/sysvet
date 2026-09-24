using Platform.Domain.Entities;

namespace Platform.Application.Status;

/// <summary>Super Admin incident row.</summary>
public sealed record StatusIncidentDto(
    Guid Id,
    string Title,
    StatusIncidentImpact Impact,
    string Components,
    DateTimeOffset StartedAt,
    DateTimeOffset? ResolvedAt);

/// <summary>Public status page payload (no PII).</summary>
public sealed record PublicStatusDto(
    string OverallStatus,
    IReadOnlyList<PublicStatusComponentDto> Components,
    IReadOnlyList<PublicStatusIncidentDto> OpenIncidents);

/// <summary>Component health on the public status page.</summary>
public sealed record PublicStatusComponentDto(string Key, string Status);

/// <summary>Open incident summary for subscribers.</summary>
public sealed record PublicStatusIncidentDto(
    Guid Id,
    string Title,
    StatusIncidentImpact Impact,
    string Components,
    DateTimeOffset StartedAt);
