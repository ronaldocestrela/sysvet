using Core.Domain;
using Core.Domain.Auditing;
using Inventory.Domain.Entities;
using Inventory.Domain.Repositories;
using MediatR;

namespace Inventory.Application.Suppliers.Commands;

public sealed class RegisterSupplierCommandHandler : IRequestHandler<RegisterSupplierCommand, Result<Guid>>
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly ITenantContext _tenantContext;
    private readonly IAuditLogger _auditLogger;

    public RegisterSupplierCommandHandler(
        ISupplierRepository supplierRepository,
        ITenantContext tenantContext,
        IAuditLogger auditLogger)
    {
        _supplierRepository = supplierRepository;
        _tenantContext = tenantContext;
        _auditLogger = auditLogger;
    }

    public async Task<Result<Guid>> Handle(RegisterSupplierCommand request, CancellationToken cancellationToken)
    {
        if (await _supplierRepository.GetByDocumentAsync(request.Document, cancellationToken) is not null)
        {
            return Result.Failure<Guid>(Inventory.Domain.ErrorCodes.Supplier.DocumentConflict);
        }

        var id = request.SupplierId == Guid.Empty ? Guid.NewGuid() : request.SupplierId;
        var created = Supplier.Create(request.LegalName, request.TradeName, request.Document, request.ContactEmail, request.ContactPhone, id);
        if (created.IsFailure)
        {
            return Result.Failure<Guid>(created.Error);
        }

        _supplierRepository.Add(created.Value);
        await _auditLogger.LogAsync(_tenantContext.TenantId, _tenantContext.UserId, created.Value.Id, "Supplier", "Register", created.Value.LegalName, cancellationToken);
        return Result.Success(created.Value.Id);
    }
}

public sealed class UpdateSupplierCommandHandler : IRequestHandler<UpdateSupplierCommand, Result>
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly ITenantContext _tenantContext;
    private readonly IAuditLogger _auditLogger;

    public UpdateSupplierCommandHandler(ISupplierRepository supplierRepository, ITenantContext tenantContext, IAuditLogger auditLogger)
    {
        _supplierRepository = supplierRepository;
        _tenantContext = tenantContext;
        _auditLogger = auditLogger;
    }

    public async Task<Result> Handle(UpdateSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _supplierRepository.GetByIdAsync(request.SupplierId, cancellationToken);
        if (supplier is null)
        {
            return Result.Failure(Inventory.Domain.ErrorCodes.Supplier.NotFound);
        }

        var update = supplier.Update(request.LegalName, request.TradeName, request.ContactEmail, request.ContactPhone);
        if (update.IsFailure)
        {
            return update;
        }

        _supplierRepository.Update(supplier);
        await _auditLogger.LogAsync(_tenantContext.TenantId, _tenantContext.UserId, supplier.Id, "Supplier", "Update", supplier.LegalName, cancellationToken);
        return Result.Success();
    }
}

public sealed class SetSupplierActiveCommandHandler : IRequestHandler<SetSupplierActiveCommand, Result>
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly ITenantContext _tenantContext;
    private readonly IAuditLogger _auditLogger;

    public SetSupplierActiveCommandHandler(ISupplierRepository supplierRepository, ITenantContext tenantContext, IAuditLogger auditLogger)
    {
        _supplierRepository = supplierRepository;
        _tenantContext = tenantContext;
        _auditLogger = auditLogger;
    }

    public async Task<Result> Handle(SetSupplierActiveCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _supplierRepository.GetByIdAsync(request.SupplierId, cancellationToken);
        if (supplier is null)
        {
            return Result.Failure(Inventory.Domain.ErrorCodes.Supplier.NotFound);
        }

        supplier.SetActive(request.IsActive);
        _supplierRepository.Update(supplier);
        await _auditLogger.LogAsync(_tenantContext.TenantId, _tenantContext.UserId, supplier.Id, "Supplier", request.IsActive ? "Activate" : "Deactivate", supplier.LegalName, cancellationToken);
        return Result.Success();
    }
}
