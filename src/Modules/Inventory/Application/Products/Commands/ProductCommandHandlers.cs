using Core.Domain;
using Core.Domain.Auditing;
using Inventory.Application.Common;
using Inventory.Domain.Entities;
using Inventory.Domain.Repositories;
using MediatR;

namespace Inventory.Application.Products.Commands;

/// <summary>Handles product write commands.</summary>
public sealed class RegisterProductCommandHandler : IRequestHandler<RegisterProductCommand, Result<Guid>>
{
    private readonly IProductRepository _productRepository;
    private readonly ISupplierRepository _supplierRepository;
    private readonly ITenantContext _tenantContext;
    private readonly IAuditLogger _auditLogger;

    public RegisterProductCommandHandler(
        IProductRepository productRepository,
        ISupplierRepository supplierRepository,
        ITenantContext tenantContext,
        IAuditLogger auditLogger)
    {
        _productRepository = productRepository;
        _supplierRepository = supplierRepository;
        _tenantContext = tenantContext;
        _auditLogger = auditLogger;
    }

    public async Task<Result<Guid>> Handle(RegisterProductCommand request, CancellationToken cancellationToken)
    {
        if (await _productRepository.GetBySkuAsync(request.Sku, cancellationToken) is not null)
        {
            return Result.Failure<Guid>(Inventory.Domain.ErrorCodes.Product.SkuConflict);
        }

        if (await _productRepository.GetByBarcodeAsync(request.Barcode, cancellationToken) is not null)
        {
            return Result.Failure<Guid>(Inventory.Domain.ErrorCodes.Product.BarcodeConflict);
        }

        if (request.SupplierId is Guid supplierId)
        {
            var supplier = await _supplierRepository.GetByIdAsync(supplierId, cancellationToken);
            if (supplier is null || !supplier.IsActive)
            {
                return Result.Failure<Guid>(Inventory.Domain.ErrorCodes.Supplier.NotFound);
            }
        }

        var productId = request.ProductId == Guid.Empty ? Guid.NewGuid() : request.ProductId;
        var productResult = Product.Create(
            request.Name,
            request.Description,
            request.Sku,
            request.Barcode,
            request.UnitOfMeasure,
            request.ReorderLevel,
            request.Category,
            request.Ncm,
            request.Cest,
            request.MerchandiseOrigin,
            request.SupplierId,
            request.RequiresLot,
            productId);

        if (productResult.IsFailure)
        {
            return Result.Failure<Guid>(productResult.Error);
        }

        var product = productResult.Value;
        var balance = new ProductBalance(product.Id, 0m);
        _productRepository.Add(product);
        await _productRepository.AddBalanceAsync(balance, cancellationToken);

        await _auditLogger.LogAsync(
            _tenantContext.TenantId,
            _tenantContext.UserId,
            product.Id,
            "Product",
            "Register",
            $"Product {product.Name} SKU {product.Sku}",
            cancellationToken);

        return Result.Success(product.Id);
    }
}

/// <summary>Updates product catalog fields.</summary>
public sealed class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result>
{
    private readonly IProductRepository _productRepository;
    private readonly ISupplierRepository _supplierRepository;
    private readonly ITenantContext _tenantContext;
    private readonly IAuditLogger _auditLogger;

    public UpdateProductCommandHandler(
        IProductRepository productRepository,
        ISupplierRepository supplierRepository,
        ITenantContext tenantContext,
        IAuditLogger auditLogger)
    {
        _productRepository = productRepository;
        _supplierRepository = supplierRepository;
        _tenantContext = tenantContext;
        _auditLogger = auditLogger;
    }

    public async Task<Result> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
        {
            return Result.Failure(Inventory.Domain.ErrorCodes.Product.NotFound);
        }

        var skuOwner = await _productRepository.GetBySkuAsync(request.Sku, cancellationToken);
        if (skuOwner is not null && skuOwner.Id != product.Id)
        {
            return Result.Failure(Inventory.Domain.ErrorCodes.Product.SkuConflict);
        }

        var barcodeOwner = await _productRepository.GetByBarcodeAsync(request.Barcode, cancellationToken);
        if (barcodeOwner is not null && barcodeOwner.Id != product.Id)
        {
            return Result.Failure(Inventory.Domain.ErrorCodes.Product.BarcodeConflict);
        }

        if (request.SupplierId is Guid supplierId)
        {
            var supplier = await _supplierRepository.GetByIdAsync(supplierId, cancellationToken);
            if (supplier is null || !supplier.IsActive)
            {
                return Result.Failure(Inventory.Domain.ErrorCodes.Supplier.NotFound);
            }
        }

        var update = product.UpdateDetails(
            request.Name,
            request.Description,
            request.Sku,
            request.Barcode,
            request.UnitOfMeasure,
            request.ReorderLevel,
            request.Category,
            request.Ncm,
            request.Cest,
            request.MerchandiseOrigin,
            request.SupplierId,
            request.RequiresLot);

        if (update.IsFailure)
        {
            return update;
        }

        _productRepository.Update(product);

        await _auditLogger.LogAsync(
            _tenantContext.TenantId,
            _tenantContext.UserId,
            product.Id,
            "Product",
            "Update",
            $"Product {product.Name} updated",
            cancellationToken);

        return Result.Success();
    }
}

/// <summary>Sets product active flag.</summary>
public sealed class SetProductActiveCommandHandler : IRequestHandler<SetProductActiveCommand, Result>
{
    private readonly IProductRepository _productRepository;
    private readonly ITenantContext _tenantContext;
    private readonly IAuditLogger _auditLogger;

    public SetProductActiveCommandHandler(
        IProductRepository productRepository,
        ITenantContext tenantContext,
        IAuditLogger auditLogger)
    {
        _productRepository = productRepository;
        _tenantContext = tenantContext;
        _auditLogger = auditLogger;
    }

    public async Task<Result> Handle(SetProductActiveCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
        {
            return Result.Failure(Inventory.Domain.ErrorCodes.Product.NotFound);
        }

        product.SetActive(request.IsActive);
        _productRepository.Update(product);

        await _auditLogger.LogAsync(
            _tenantContext.TenantId,
            _tenantContext.UserId,
            product.Id,
            "Product",
            request.IsActive ? "Activate" : "Deactivate",
            $"Product {product.Name}",
            cancellationToken);

        return Result.Success();
    }
}
