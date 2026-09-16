using Core.Domain;
using Core.Domain.Auditing;
using Inventory.Domain.Entities;
using Inventory.Domain.Repositories;
using MediatR;

namespace Inventory.Application.Products.Commands;

public class RegisterProductCommandHandler : IRequestHandler<RegisterProductCommand, Result<Guid>>
{
    private readonly IProductRepository _productRepository;
    private readonly ITenantContext _tenantContext;
    private readonly IAuditLogger _auditLogger;

    public RegisterProductCommandHandler(
        IProductRepository productRepository,
        ITenantContext tenantContext,
        IAuditLogger auditLogger)
    {
        _productRepository = productRepository;
        _tenantContext = tenantContext;
        _auditLogger = auditLogger;
    }

    public async Task<Result<Guid>> Handle(RegisterProductCommand request, CancellationToken cancellationToken)
    {
        var existingProduct = await _productRepository.GetByBarcodeAsync(request.Barcode, cancellationToken);
        if (existingProduct != null)
        {
            return Result.Failure<Guid>(Inventory.Domain.ErrorCodes.Product.BarcodeConflict);
        }

        var productResult = Product.Create(
            request.Name,
            request.Description,
            request.Barcode,
            request.UnitOfMeasure,
            request.ReorderLevel);

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
            $"Product {product.Name} registered with barcode {product.Barcode}",
            cancellationToken);

        return Result.Success(product.Id);
    }
}
