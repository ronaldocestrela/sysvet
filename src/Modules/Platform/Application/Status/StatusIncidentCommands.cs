using Core.Application.Messaging;
using Platform.Domain.Entities;

namespace Platform.Application.Status;

/// <summary>Opens a new public status incident.</summary>
public sealed record CreateStatusIncidentCommand(
    string Title,
    StatusIncidentImpact Impact,
    string Components) : ICommand<StatusIncidentDto>;

/// <summary>Resolves an open incident.</summary>
public sealed record ResolveStatusIncidentCommand(Guid IncidentId) : ICommand<StatusIncidentDto>;
