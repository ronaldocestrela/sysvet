using Core.Application.Messaging;
using Platform.Domain.Entities;

namespace Platform.Application.Tenants.Commands;

/// <summary>Assigns a tenant to an operational release ring (ADR-060).</summary>
public sealed record SetTenantReleaseRingCommand(Guid TenantId, ReleaseRing Ring) : ICommand;
