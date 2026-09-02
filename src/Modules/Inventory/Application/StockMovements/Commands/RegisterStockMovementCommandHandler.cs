using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Domain;
using Core.Domain.Auditing;
using Inventory.Domain.Entities;
using Inventory.Domain.Repositories;
using MediatR;

namespace Inventory.Application.StockMovements.Commands;

public class RegisterStockMovementCommandHandler : IRequestHandler<RegisterStockMovementCommand, Result<Guid>>
{
    private readonly IProductRepository _productRepository;
    private readonly IStockMovementRepository _movementRepository;
    private readonly IInventoryUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly IAuditLogger _auditLogger;

    public RegisterStockMovementCommandHandler(
        IProductRepository productRepository, 
        IStockMovementRepository movementRepository,
        IInventoryUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        IAuditLogger auditLogger)
    {
        _productRepository = productRepository;
        _movementRepository = movementRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _auditLogger = auditLogger;
    }

    public async Task<Result<Guid>> Handle(RegisterStockMovementCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
        {
            return Result.Failure<Guid>(new Error("Product.NotFound", "The specified product was not found."));
        }

        // Create the movement log (immutable)
        var movementResult = StockMovement.Create(
            request.ProductId, 
            request.Type, 
            request.Quantity, 
            request.BatchNumber, 
            request.ExpirationDate, 
            request.Reason);

        if (movementResult.IsFailure)
        {
            return Result.Failure<Guid>(movementResult.Error);
        }

        var movement = movementResult.Value;

        // Update Balance
        var balance = await _productRepository.GetBalanceAsync(request.ProductId, cancellationToken);
        if (balance == null)
        {
            // Fallback, create a new balance row if one didn't exist for some reason
            balance = new ProductBalance(request.ProductId, 0m);
            await _productRepository.AddBalanceAsync(balance, cancellationToken);
        }

        var updateBalanceResult = balance.UpdateBalance(movement.Quantity, movement.Type);
        if (updateBalanceResult.IsFailure)
        {
            return Result.Failure<Guid>(updateBalanceResult.Error);
        }

        _movementRepository.Add(movement);
        await _productRepository.UpdateBalanceAsync(balance, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            _tenantContext.TenantId, 
            _tenantContext.UserId, 
            "StockMovement", 
            "Register", 
            $"Movement {movement.Type} of {movement.Quantity} for product {product.Name}", 
            cancellationToken);

        return Result.Success(movement.Id);
    }
}
