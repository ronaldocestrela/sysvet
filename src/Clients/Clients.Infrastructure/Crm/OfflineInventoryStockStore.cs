using Clients.Infrastructure.Sync;
using Core.Domain;
using Inventory.Domain.Entities;
using Inventory.Domain.Services;
using Microsoft.EntityFrameworkCore;
using InvErrorCodes = Inventory.Domain.ErrorCodes;

namespace Clients.Infrastructure.Crm;

public sealed partial class OfflineInventoryStore
{
    public async Task<Result<Guid>> RegisterStockMovementAsync(
        Guid productId,
        MovementType type,
        decimal quantity,
        string reason,
        Guid? productLotId = null,
        AdjustmentDirection? adjustmentDirection = null,
        CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
        if (product is null || !product.IsActive)
        {
            return Result.Failure<Guid>(InvErrorCodes.Product.NotFound);
        }

        var lots = await _dbContext.ProductLots.Where(l => l.ProductId == productId).ToListAsync(cancellationToken);
        ProductLot? lot = null;
        if (productLotId is Guid lotId)
        {
            lot = lots.FirstOrDefault(l => l.Id == lotId);
        }

        var context = StockQuantityApplier.ValidateLotContext(product, lot, productLotId, lots.Count > 0);
        if (context.IsFailure)
        {
            return Result.Failure<Guid>(context.Error);
        }

        var deltaResult = StockQuantityApplier.ResolveDelta(type, quantity, adjustmentDirection);
        if (deltaResult.IsFailure)
        {
            return Result.Failure<Guid>(deltaResult.Error);
        }

        var movementId = Guid.NewGuid();
        var useLotPath = product.RequiresLot || lots.Count > 0;
        if (useLotPath)
        {
            var adjust = lot!.AdjustQuantity(deltaResult.Value);
            if (adjust.IsFailure)
            {
                return Result.Failure<Guid>(adjust.Error);
            }

            _dbContext.ProductLots.Update(lot);
            await ReconcileLocalAsync(product, lots, cancellationToken);
        }
        else
        {
            var balance = await _dbContext.ProductBalances.FirstOrDefaultAsync(b => b.ProductId == productId, cancellationToken);
            if (balance is null)
            {
                balance = new ProductBalance(productId, 0m);
                await _dbContext.ProductBalances.AddAsync(balance, cancellationToken);
            }

            var update = balance.ApplySignedDelta(deltaResult.Value);
            if (update.IsFailure)
            {
                return Result.Failure<Guid>(update.Error);
            }

            _dbContext.ProductBalances.Update(balance);
        }

        var movement = StockMovement.Create(
            productId,
            type,
            quantity,
            lot?.LotNumber,
            lot?.ExpirationDate,
            reason,
            lot?.Id,
            adjustmentDirection,
            movementId);

        if (movement.IsFailure)
        {
            return Result.Failure<Guid>(movement.Error);
        }

        _dbContext.StockMovements.Add(movement.Value);
        var outboxId = Guid.NewGuid();
        EnqueueOutbox("RegisterStockMovementCommand",
            OutboxPayloadFactory.RegisterStockMovement(
                productId,
                type,
                quantity,
                reason,
                productLotId,
                adjustmentDirection,
                movementId,
                outboxId));
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(movementId);
    }

    public async Task<Result<Guid>> TransferStockAsync(
        Guid productId,
        Guid sourceLotId,
        Guid destinationLotId,
        decimal quantity,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
        if (product is null || !product.IsActive)
        {
            return Result.Failure<Guid>(InvErrorCodes.Product.NotFound);
        }

        var source = await _dbContext.ProductLots.FirstOrDefaultAsync(l => l.Id == sourceLotId, cancellationToken);
        var destination = await _dbContext.ProductLots.FirstOrDefaultAsync(l => l.Id == destinationLotId, cancellationToken);
        if (source is null || destination is null)
        {
            return Result.Failure<Guid>(InvErrorCodes.ProductLot.NotFound);
        }

        var validate = StockQuantityApplier.ValidateTransferLots(product, source, destination);
        if (validate.IsFailure)
        {
            return Result.Failure<Guid>(validate.Error);
        }

        if (quantity <= 0)
        {
            return Result.Failure<Guid>(InvErrorCodes.StockMovement.InvalidQuantity);
        }

        var outAdjust = source.AdjustQuantity(-quantity);
        if (outAdjust.IsFailure)
        {
            return Result.Failure<Guid>(outAdjust.Error);
        }

        var inAdjust = destination.AdjustQuantity(quantity);
        if (inAdjust.IsFailure)
        {
            return Result.Failure<Guid>(inAdjust.Error);
        }

        _dbContext.ProductLots.Update(source);
        _dbContext.ProductLots.Update(destination);

        var correlationId = Guid.NewGuid();
        var transferReason = string.IsNullOrWhiteSpace(reason) ? Inventory.Domain.StockMovementReasons.Transfer : reason;
        var outMovement = StockMovement.Create(productId, MovementType.Out, quantity, source.LotNumber, source.ExpirationDate, transferReason, source.Id, correlationId: correlationId);
        var inMovement = StockMovement.Create(productId, MovementType.In, quantity, destination.LotNumber, destination.ExpirationDate, transferReason, destination.Id, correlationId: correlationId);
        if (outMovement.IsFailure || inMovement.IsFailure)
        {
            return Result.Failure<Guid>(outMovement.IsFailure ? outMovement.Error : inMovement.Error!);
        }

        _dbContext.StockMovements.Add(outMovement.Value);
        _dbContext.StockMovements.Add(inMovement.Value);
        var lots = await _dbContext.ProductLots.Where(l => l.ProductId == productId).ToListAsync(cancellationToken);
        await ReconcileLocalAsync(product, lots, cancellationToken);

        var outboxId = Guid.NewGuid();
        EnqueueOutbox("TransferStockCommand",
            OutboxPayloadFactory.TransferStock(productId, sourceLotId, destinationLotId, quantity, transferReason, correlationId, outboxId));
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(correlationId);
    }

    public async Task<Result<Guid>> RegisterStockLossAsync(
        Guid productId,
        Guid? productLotId,
        decimal quantity,
        string lossReasonCode,
        string? notes = null,
        CancellationToken cancellationToken = default)
    {
        if (!Inventory.Domain.StockLossReasons.IsValid(lossReasonCode))
        {
            return Result.Failure<Guid>(InvErrorCodes.StockMovement.InvalidLossReason);
        }

        var reason = Inventory.Domain.StockLossReasons.ToMovementReason(lossReasonCode);
        return await ApplyStockOutAsync(
            productId,
            productLotId,
            quantity,
            reason,
            notes,
            null,
            "RegisterStockLossCommand",
            cancellationToken,
            (movementId, outboxId) => OutboxPayloadFactory.RegisterStockLoss(productId, productLotId, quantity, lossReasonCode, notes, movementId, outboxId));
    }

    public async Task<Result<Guid>> FractionatePackageAsync(
        Guid productId,
        Guid sealedLotId,
        decimal packagesToOpen,
        CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
        if (product is null || !product.IsActive)
        {
            return Result.Failure<Guid>(InvErrorCodes.Product.NotFound);
        }

        var sealedLot = await _dbContext.ProductLots.FirstOrDefaultAsync(l => l.Id == sealedLotId, cancellationToken);
        if (sealedLot is null)
        {
            return Result.Failure<Guid>(InvErrorCodes.ProductLot.NotFound);
        }

        var planResult = PackageFractionationService.Plan(product, sealedLot, packagesToOpen);
        if (planResult.IsFailure)
        {
            return Result.Failure<Guid>(planResult.Error);
        }

        var plan = planResult.Value;
        var destination = await _dbContext.ProductLots.FirstOrDefaultAsync(
            l => l.ProductId == productId && l.LotNumber == plan.TargetLotNumber,
            cancellationToken);

        if (destination is null)
        {
            var create = ProductLot.Create(product.Id, plan.TargetLotNumber, plan.ExpirationDate, plan.UnitCost, 0m, isFractional: true);
            if (create.IsFailure)
            {
                return Result.Failure<Guid>(create.Error);
            }

            destination = create.Value;
            _dbContext.ProductLots.Add(destination);
        }

        var validate = StockQuantityApplier.ValidateTransferLots(product, sealedLot, destination);
        if (validate.IsFailure)
        {
            return Result.Failure<Guid>(validate.Error);
        }

        sealedLot.AdjustQuantity(-plan.QuantityToTransfer);
        destination.AdjustQuantity(plan.QuantityToTransfer);
        _dbContext.ProductLots.Update(sealedLot);
        _dbContext.ProductLots.Update(destination);

        var correlationId = Guid.NewGuid();
        var reason = Inventory.Domain.StockMovementReasons.Fractionation;
        var outMovement = StockMovement.Create(productId, MovementType.Out, plan.QuantityToTransfer, sealedLot.LotNumber, sealedLot.ExpirationDate, reason, sealedLot.Id, correlationId: correlationId);
        var inMovement = StockMovement.Create(productId, MovementType.In, plan.QuantityToTransfer, destination.LotNumber, destination.ExpirationDate, reason, destination.Id, correlationId: correlationId);
        if (outMovement.IsFailure || inMovement.IsFailure)
        {
            return Result.Failure<Guid>(outMovement.IsFailure ? outMovement.Error : inMovement.Error!);
        }

        _dbContext.StockMovements.Add(outMovement.Value);
        _dbContext.StockMovements.Add(inMovement.Value);
        var lots = await _dbContext.ProductLots.Where(l => l.ProductId == productId).ToListAsync(cancellationToken);
        await ReconcileLocalAsync(product, lots, cancellationToken);

        var outboxId = Guid.NewGuid();
        EnqueueOutbox("FractionatePackageCommand",
            OutboxPayloadFactory.FractionatePackage(productId, sealedLotId, packagesToOpen, correlationId, outboxId));
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(correlationId);
    }

    public async Task<Result<Guid>> RegisterSupplierReturnAsync(
        Guid productId,
        Guid? productLotId,
        decimal quantity,
        Guid? supplierId,
        string? notes = null,
        CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
        if (product is null || !product.IsActive)
        {
            return Result.Failure<Guid>(InvErrorCodes.Product.NotFound);
        }

        var resolvedSupplier = supplierId ?? product.SupplierId;
        if (resolvedSupplier is null || resolvedSupplier == Guid.Empty)
        {
            return Result.Failure<Guid>(InvErrorCodes.StockMovement.SupplierRequired);
        }

        if (!await _dbContext.Suppliers.AnyAsync(s => s.Id == resolvedSupplier && s.IsActive, cancellationToken))
        {
            return Result.Failure<Guid>(InvErrorCodes.Supplier.NotFound);
        }

        var reason = Inventory.Domain.StockMovementReasons.SupplierReturn;
        return await ApplyStockOutAsync(
            productId,
            productLotId,
            quantity,
            reason,
            notes,
            resolvedSupplier,
            "RegisterSupplierReturnCommand",
            cancellationToken,
            (movementId, outboxId) => OutboxPayloadFactory.RegisterSupplierReturn(
                productId, productLotId, quantity, resolvedSupplier, notes, movementId, outboxId));
    }

    private async Task<Result<Guid>> ApplyStockOutAsync(
        Guid productId,
        Guid? productLotId,
        decimal quantity,
        string reason,
        string? notes,
        Guid? supplierId,
        string outboxType,
        CancellationToken cancellationToken,
        Func<Guid, Guid, string> outboxPayload)
    {
        var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
        if (product is null || !product.IsActive)
        {
            return Result.Failure<Guid>(InvErrorCodes.Product.NotFound);
        }

        var lots = await _dbContext.ProductLots.Where(l => l.ProductId == productId).ToListAsync(cancellationToken);
        ProductLot? lot = null;
        if (productLotId is Guid lotId)
        {
            lot = lots.FirstOrDefault(l => l.Id == lotId);
        }

        var context = StockQuantityApplier.ValidateLotContext(product, lot, productLotId, lots.Count > 0);
        if (context.IsFailure)
        {
            return Result.Failure<Guid>(context.Error);
        }

        var deltaResult = StockQuantityApplier.ResolveDelta(MovementType.Out, quantity, null);
        if (deltaResult.IsFailure)
        {
            return Result.Failure<Guid>(deltaResult.Error);
        }

        var movementId = Guid.NewGuid();
        var useLotPath = product.RequiresLot || lots.Count > 0;
        if (useLotPath)
        {
            var adjust = lot!.AdjustQuantity(deltaResult.Value);
            if (adjust.IsFailure)
            {
                return Result.Failure<Guid>(adjust.Error);
            }

            _dbContext.ProductLots.Update(lot);
            await ReconcileLocalAsync(product, lots, cancellationToken);
        }
        else
        {
            var balance = await _dbContext.ProductBalances.FirstOrDefaultAsync(b => b.ProductId == productId, cancellationToken);
            if (balance is null)
            {
                balance = new ProductBalance(productId, 0m);
                await _dbContext.ProductBalances.AddAsync(balance, cancellationToken);
            }

            var update = balance.ApplySignedDelta(deltaResult.Value);
            if (update.IsFailure)
            {
                return Result.Failure<Guid>(update.Error);
            }

            _dbContext.ProductBalances.Update(balance);
        }

        var movement = StockMovement.Create(
            productId,
            MovementType.Out,
            quantity,
            lot?.LotNumber,
            lot?.ExpirationDate,
            reason,
            lot?.Id,
            notes: notes,
            id: movementId,
            supplierId: supplierId);

        if (movement.IsFailure)
        {
            return Result.Failure<Guid>(movement.Error);
        }

        _dbContext.StockMovements.Add(movement.Value);
        var outboxId = Guid.NewGuid();
        EnqueueOutbox(outboxType, outboxPayload(movementId, outboxId));
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(movementId);
    }

    public async Task<Result<IReadOnlyList<InventoryStockMovementItem>>> ListStockMovementsAsync(
        Guid? productId = null,
        int take = 50,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.StockMovements.AsNoTracking().AsQueryable();
        if (productId is Guid pid)
        {
            query = query.Where(m => m.ProductId == pid);
        }

        var movements = await query.OrderByDescending(m => m.Date).Take(Math.Clamp(take, 1, 200)).ToListAsync(cancellationToken);
        var productNames = await _dbContext.Products.AsNoTracking().ToDictionaryAsync(p => p.Id, p => p.Name, cancellationToken);
        var lotNumbers = await _dbContext.ProductLots.AsNoTracking().ToDictionaryAsync(l => l.Id, l => l.LotNumber, cancellationToken);

        var items = movements.Select(m => new InventoryStockMovementItem
        {
            Id = m.Id,
            ProductId = m.ProductId,
            ProductName = productNames.GetValueOrDefault(m.ProductId) ?? string.Empty,
            Type = m.Type,
            Quantity = m.Quantity,
            Reason = m.Reason,
            LotNumber = m.ProductLotId is Guid lotId ? lotNumbers.GetValueOrDefault(lotId) : m.BatchNumber,
            Date = m.Date
        }).ToList();

        return Result.Success<IReadOnlyList<InventoryStockMovementItem>>(items);
    }

    public async Task<Result<IReadOnlyList<InventoryKardexLine>>> GetProductKardexAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        if (!await _dbContext.Products.AnyAsync(p => p.Id == productId, cancellationToken))
        {
            return Result.Failure<IReadOnlyList<InventoryKardexLine>>(InvErrorCodes.Product.NotFound);
        }

        var movements = await _dbContext.StockMovements.AsNoTracking()
            .Where(m => m.ProductId == productId)
            .OrderBy(m => m.Date)
            .ThenBy(m => m.Id)
            .ToListAsync(cancellationToken);

        var lotNumbers = await _dbContext.ProductLots.AsNoTracking()
            .Where(l => l.ProductId == productId)
            .ToDictionaryAsync(l => l.Id, l => l.LotNumber, cancellationToken);

        decimal running = 0m;
        var lines = new List<InventoryKardexLine>();
        foreach (var movement in movements)
        {
            var delta = StockQuantityApplier.ResolveDelta(movement.Type, movement.Quantity, movement.AdjustmentDirection);
            if (delta.IsSuccess)
            {
                running += delta.Value;
            }

            lines.Add(new InventoryKardexLine
            {
                Date = movement.Date,
                Type = movement.Type,
                Quantity = movement.Quantity,
                RunningBalance = running,
                Reason = movement.Reason,
                LotNumber = movement.ProductLotId is Guid lotId ? lotNumbers.GetValueOrDefault(lotId) : movement.BatchNumber
            });
        }

        return Result.Success<IReadOnlyList<InventoryKardexLine>>(lines);
    }

    public async Task<Result<IReadOnlyList<InventoryStockAlertItem>>> GetStockAlertsAsync(
        StockAlertKind? kind = null,
        int horizonDays = 30,
        CancellationToken cancellationToken = default)
    {
        var horizon = Math.Clamp(horizonDays, 1, 365);
        var utcNow = DateTimeOffset.UtcNow;
        var products = await _dbContext.Products.AsNoTracking().Where(p => p.IsActive).ToListAsync(cancellationToken);
        var alerts = new List<InventoryStockAlertItem>();

        foreach (var product in products)
        {
            var lots = await _dbContext.ProductLots.AsNoTracking().Where(l => l.ProductId == product.Id).ToListAsync(cancellationToken);
            var totalFromLots = lots.Where(l => l.IsActive).Sum(l => l.Quantity);
            var balance = await _dbContext.ProductBalances.AsNoTracking().FirstOrDefaultAsync(b => b.ProductId == product.Id, cancellationToken);
            var total = lots.Count > 0 ? totalFromLots : balance?.TotalQuantity ?? 0m;

            if (kind is null or StockAlertKind.LowStock && StockAlertClassifier.IsLowStock(total, product.ReorderLevel))
            {
                alerts.Add(new InventoryStockAlertItem
                {
                    Kind = StockAlertKind.LowStock,
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Sku = product.Sku,
                    TotalQuantity = total,
                    ReorderLevel = product.ReorderLevel
                });
            }

            foreach (var lot in lots.Where(l => l.IsActive && l.Quantity > 0))
            {
                var expiryKind = StockAlertClassifier.ClassifyLotExpiry(lot.ExpirationDate, utcNow, horizon);
                if (expiryKind == StockAlertKind.None || (kind is not null && kind != expiryKind))
                {
                    continue;
                }

                alerts.Add(new InventoryStockAlertItem
                {
                    Kind = expiryKind,
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Sku = product.Sku,
                    LotNumber = lot.LotNumber,
                    TotalQuantity = lot.Quantity,
                    ReorderLevel = product.ReorderLevel,
                    ExpirationDate = lot.ExpirationDate
                });
            }
        }

        return Result.Success<IReadOnlyList<InventoryStockAlertItem>>(alerts);
    }

    private async Task ReconcileLocalAsync(Product product, List<ProductLot> lots, CancellationToken cancellationToken)
    {
        var balance = await _dbContext.ProductBalances.FirstOrDefaultAsync(b => b.ProductId == product.Id, cancellationToken);
        if (balance is null)
        {
            balance = new ProductBalance(product.Id, 0m);
            await _dbContext.ProductBalances.AddAsync(balance, cancellationToken);
        }

        balance.SyncFromLots(InventoryCostCalculator.TotalQuantityFromLots(lots.Select(l => (l.Quantity, l.IsActive))));
        product.RecalculateAverageCost(InventoryCostCalculator.WeightedAverageCost(lots.Where(l => l.IsActive).Select(l => (l.Quantity, l.UnitCost))));
        _dbContext.Products.Update(product);
        _dbContext.ProductBalances.Update(balance);
    }
}
