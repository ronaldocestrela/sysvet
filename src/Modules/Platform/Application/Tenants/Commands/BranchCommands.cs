using Core.Application.Messaging;
using Platform.Application.Tenants.Dtos;

namespace Platform.Application.Tenants.Commands;

/// <summary>Adds a branch CNPJ to an existing tenant.</summary>
public sealed record AddBranchCommand(
    Guid TenantId,
    string Cnpj,
    string LegalName,
    bool IsHeadquarters = false) : ICommand<BranchDto>;

/// <summary>Updates branch legal name.</summary>
public sealed record UpdateBranchCommand(Guid TenantId, Guid BranchId, string LegalName) : ICommand<BranchDto>;

/// <summary>Soft-deletes a branch.</summary>
public sealed record DeleteBranchCommand(Guid TenantId, Guid BranchId) : ICommand;
