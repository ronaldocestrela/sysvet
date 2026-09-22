using Core.Application.Messaging;
using Platform.Domain.Entities;

namespace Platform.Application.Tenants.Commands;

/// <summary>Changes tenant lifecycle status from Super Admin.</summary>
public sealed record ChangeTenantStatusCommand(Guid TenantId, TenantStatus Status) : ICommand;

/// <summary>Soft-deletes a tenant catalog row.</summary>
public sealed record DeleteTenantCommand(Guid TenantId) : ICommand;
