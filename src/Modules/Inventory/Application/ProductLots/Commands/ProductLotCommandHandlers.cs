using Core.Domain;
using Core.Domain.Auditing;
using Inventory.Application.Common;
using Inventory.Domain;
using Inventory.Domain.Entities;
using Inventory.Domain.Repositories;
using MediatR;

namespace Inventory.Application.ProductLots.Commands;

public sealed class RegisterProductLotCommandHandler : IRequestHandler<RegisterProductLotCommand, Result<Guid>>
{
    private readonly IProductRepository _productRepository;
    private readonly IProductLotRepository _lotRepository;
    private readonly IStockMovementRepository _movementRepository;
    private readonly ITenantContext _tenantContext;
    private readonly IAuditLogger _auditLogger;

    public RegisterProductLotCommandHandler(
        IProductRepository productRepository,
        IProductLotRepository lotRepository,
        IStockMovementRepository movementRepository,
        ITenantContext tenantContext,
        IAuditLogger auditLogger)
    {
        _productRepository = productRepository;
        _lotRepository = lotRepository;
        _movementRepository = movementRepository;
        _tenantContext = tenantContext;
        _auditLogger = auditLogger;
    }

    public async Task<Result<Guid>> Handle(RegisterProductLotCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null || !product.IsActive)
        {
            return Result.Failure<Guid>(Inventory.Domain.ErrorCodes.Product.NotFound);
        }

        if (await _lotRepository.GetByProductAndLotNumberAsync(request.ProductId, request.LotNumber, cancellationToken) is not null)
        {
            return Result.Failure<Guid>(Inventory.Domain.ErrorCodes.ProductLot.LotNumberConflict);
        }

        var lotId = request.LotId == Guid.Empty ? Guid.NewGuid() : request.LotId;
        var lotResult = ProductLot.Create(request.ProductId, request.LotNumber, request.ExpirationDate, request.UnitCost, request.InitialQuantity, lotId);
        if (lotResult.IsFailure)
        {
            return Result.Failure<Guid>(lotResult.Error);
        }

        var lot = lotResult.Value;
        _lotRepository.Add(lot);

        if (request.InitialQuantity > 0)
        {
            var movement = StockMovement.Create(
                product.Id,
                MovementType.In,
                request.InitialQuantity,
                lot.LotNumber,
                lot.ExpirationDate,
                StockMovementReasons.OpeningBalance,
                lot.Id);
            if (movement.IsFailure)
            {
                return Result.Failure<Guid>(movement.Error);
            }

            _movementRepository.Add(movement.Value);
        }

        await RecalculateAsync(product, cancellationToken);

        await _auditLogger.LogAsync(
            _tenantContext.TenantId,
            _tenantContext.UserId,
            lot.Id,
            "ProductLot",
            "Register",
            $"Lot {lot.LotNumber} for product {product.Sku}",
            cancellationToken);

        return Result.Success(lot.Id);
    }

    private async Task RecalculateAsync(Product product, CancellationToken cancellationToken)
    {
        var balance = await _productRepository.GetBalanceAsync(product.Id, cancellationToken);
        if (balance is null)
        {
            balance = new ProductBalance(product.Id, 0m);
            await _productRepository.AddBalanceAsync(balance, cancellationToken);
        }

        var lots = await _lotRepository.ListByProductIdAsync(product.Id, cancellationToken);
        ProductCatalogProjection.ApplyLotSnapshot(product, balance, lots);
        _productRepository.Update(product);
        await _productRepository.UpdateBalanceAsync(balance, cancellationToken);
    }
}

public sealed class UpdateProductLotCommandHandler : IRequestHandler<UpdateProductLotCommand, Result>
{
    private readonly IProductRepository _productRepository;
    private readonly IProductLotRepository _lotRepository;
    private readonly ITenantContext _tenantContext;
    private readonly IAuditLogger _auditLogger;

    public UpdateProductLotCommandHandler(
        IProductRepository productRepository,
        IProductLotRepository lotRepository,
        ITenantContext tenantContext,
        IAuditLogger auditLogger)
    {
        _productRepository = productRepository;
        _lotRepository = lotRepository;
        _tenantContext = tenantContext;
        _auditLogger = auditLogger;
    }

    public async Task<Result> Handle(UpdateProductLotCommand request, CancellationToken cancellationToken)
    {
        var lot = await _lotRepository.GetByIdAsync(request.LotId, cancellationToken);
        if (lot is null)
        {
            return Result.Failure(Inventory.Domain.ErrorCodes.ProductLot.NotFound);
        }

        var update = lot.UpdateMetadata(request.ExpirationDate, request.UnitCost);
        if (update.IsFailure)
        {
            return update;
        }

        _lotRepository.Update(lot);
        var product = await _productRepository.GetByIdAsync(lot.ProductId, cancellationToken);
        if (product is not null)
        {
            var balance = await _productRepository.GetBalanceAsync(product.Id, cancellationToken);
            if (balance is not null)
            {
                var lots = await _lotRepository.ListByProductIdAsync(product.Id, cancellationToken);
                ProductCatalogProjection.ApplyLotSnapshot(product, balance, lots);
                _productRepository.Update(product);
                await _productRepository.UpdateBalanceAsync(balance, cancellationToken);
            }
        }

        await _auditLogger.LogAsync(_tenantContext.TenantId, _tenantContext.UserId, lot.Id, "ProductLot", "Update", lot.LotNumber, cancellationToken);
        return Result.Success();
    }
}

public sealed class SetProductLotActiveCommandHandler : IRequestHandler<SetProductLotActiveCommand, Result>
{
    private readonly IProductRepository _productRepository;
    private readonly IProductLotRepository _lotRepository;
    private readonly ITenantContext _tenantContext;
    private readonly IAuditLogger _auditLogger;

    public SetProductLotActiveCommandHandler(
        IProductRepository productRepository,
        IProductLotRepository lotRepository,
        ITenantContext tenantContext,
        IAuditLogger auditLogger)
    {
        _productRepository = productRepository;
        _lotRepository = lotRepository;
        _tenantContext = tenantContext;
        _auditLogger = auditLogger;
    }

    public async Task<Result> Handle(SetProductLotActiveCommand request, CancellationToken cancellationToken)
    {
        var lot = await _lotRepository.GetByIdAsync(request.LotId, cancellationToken);
        if (lot is null)
        {
            return Result.Failure(Inventory.Domain.ErrorCodes.ProductLot.NotFound);
        }

        lot.SetActive(request.IsActive);
        _lotRepository.Update(lot);

        var product = await _productRepository.GetByIdAsync(lot.ProductId, cancellationToken);
        if (product is not null)
        {
            var balance = await _productRepository.GetBalanceAsync(product.Id, cancellationToken);
            if (balance is not null)
            {
                var lots = await _lotRepository.ListByProductIdAsync(product.Id, cancellationToken);
                ProductCatalogProjection.ApplyLotSnapshot(product, balance, lots);
                _productRepository.Update(product);
                await _productRepository.UpdateBalanceAsync(balance, cancellationToken);
            }
        }

        await _auditLogger.LogAsync(_tenantContext.TenantId, _tenantContext.UserId, lot.Id, "ProductLot", request.IsActive ? "Activate" : "Deactivate", lot.LotNumber, cancellationToken);
        return Result.Success();
    }
}
