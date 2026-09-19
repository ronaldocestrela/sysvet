using Core.Domain;
using Core.Domain.Auditing;
using Inventory.Application.Common;
using Inventory.Domain;
using Inventory.Domain.Entities;
using InvErrors = Inventory.Domain.ErrorCodes;
using Inventory.Domain.Repositories;
using Inventory.Domain.ValueObjects;
using MediatR;

namespace Inventory.Application.InventoryCounts.Commands;

/// <summary>Starts a new inventory count when none is in progress.</summary>
public sealed class StartInventoryCountCommandHandler : IRequestHandler<StartInventoryCountCommand, Result<Guid>>
{
    private readonly IInventoryCountRepository _countRepository;
    private readonly ITenantContext _tenantContext;
    private readonly IAuditLogger _auditLogger;

    public StartInventoryCountCommandHandler(
        IInventoryCountRepository countRepository,
        ITenantContext tenantContext,
        IAuditLogger auditLogger)
    {
        _countRepository = countRepository;
        _tenantContext = tenantContext;
        _auditLogger = auditLogger;
    }

    public async Task<Result<Guid>> Handle(StartInventoryCountCommand request, CancellationToken cancellationToken)
    {
        var inProgress = await _countRepository.GetInProgressAsync(cancellationToken);
        if (inProgress is not null)
        {
            return Result.Failure<Guid>(InvErrors.InventoryCount.AlreadyInProgress);
        }

        var code = $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..4].ToUpperInvariant()}";
        var sessionResult = InventoryCount.Start(code);
        if (sessionResult.IsFailure)
        {
            return Result.Failure<Guid>(sessionResult.Error);
        }

        var session = sessionResult.Value;
        _countRepository.Add(session);

        await _auditLogger.LogAsync(
            _tenantContext.TenantId,
            _tenantContext.UserId,
            session.Id,
            nameof(InventoryCount),
            "Start",
            code,
            cancellationToken);

        return Result.Success(session.Id);
    }
}

/// <summary>Resolves product and adds or increments a count line.</summary>
public sealed class AddInventoryCountLineCommandHandler : IRequestHandler<AddInventoryCountLineCommand, Result<Guid>>
{
    private readonly IInventoryCountRepository _countRepository;
    private readonly IProductRepository _productRepository;
    private readonly IProductLotRepository _lotRepository;

    public AddInventoryCountLineCommandHandler(
        IInventoryCountRepository countRepository,
        IProductRepository productRepository,
        IProductLotRepository lotRepository)
    {
        _countRepository = countRepository;
        _productRepository = productRepository;
        _lotRepository = lotRepository;
    }

    public async Task<Result<Guid>> Handle(AddInventoryCountLineCommand request, CancellationToken cancellationToken)
    {
        var session = await _countRepository.GetByIdWithLinesAsync(request.InventoryCountId, cancellationToken);
        if (session is null)
        {
            return Result.Failure<Guid>(InvErrors.InventoryCount.NotFound);
        }

        var productResult = await ResolveProductAsync(request, cancellationToken);
        if (productResult.IsFailure)
        {
            return Result.Failure<Guid>(productResult.Error);
        }

        var product = productResult.Value;
        var lots = await _lotRepository.ListByProductIdAsync(product.Id, cancellationToken);
        if ((product.RequiresLot || lots.Count > 0) && request.ProductLotId is null)
        {
            return Result.Failure<Guid>(InvErrors.InventoryCount.LotRequired);
        }

        if (request.ProductLotId is not null)
        {
            var lot = lots.FirstOrDefault(l => l.Id == request.ProductLotId);
            if (lot is null || lot.ProductId != product.Id)
            {
                return Result.Failure<Guid>(InvErrors.ProductLot.NotFound);
            }
        }

        var lineCountBefore = session.Lines.Count;
        var lineResult = session.AddOrIncrementLine(product.Id, request.ProductLotId, request.QuantityToAdd);
        if (lineResult.IsFailure)
        {
            return lineResult;
        }

        if (session.Lines.Count > lineCountBefore)
        {
            var line = session.Lines.First(l => l.Id == lineResult.Value);
            _countRepository.StageLine(line);
        }

        return lineResult;
    }

    private async Task<Result<Product>> ResolveProductAsync(AddInventoryCountLineCommand request, CancellationToken cancellationToken)
    {
        if (request.ProductId is not null)
        {
            var byId = await _productRepository.GetByIdAsync(request.ProductId.Value, cancellationToken);
            return byId is null || !byId.IsActive
                ? Result.Failure<Product>(InvErrors.Product.NotFound)
                : Result.Success(byId);
        }

        if (string.IsNullOrWhiteSpace(request.Barcode))
        {
            return Result.Failure<Product>(InvErrors.Product.NotFound);
        }

        var barcodeResult = Barcode.Create(request.Barcode);
        if (barcodeResult.IsFailure)
        {
            return Result.Failure<Product>(barcodeResult.Error);
        }

        var product = await _productRepository.GetByBarcodeAsync(barcodeResult.Value.Value, cancellationToken);
        return product is null || !product.IsActive
            ? Result.Failure<Product>(InvErrors.Product.NotFound)
            : Result.Success(product);
    }
}

/// <summary>Updates counted quantity on a line.</summary>
public sealed class UpdateInventoryCountLineCommandHandler : IRequestHandler<UpdateInventoryCountLineCommand, Result>
{
    private readonly IInventoryCountRepository _countRepository;

    public UpdateInventoryCountLineCommandHandler(IInventoryCountRepository countRepository) => _countRepository = countRepository;

    public async Task<Result> Handle(UpdateInventoryCountLineCommand request, CancellationToken cancellationToken)
    {
        var session = await _countRepository.GetByIdWithLinesAsync(request.InventoryCountId, cancellationToken);
        if (session is null)
        {
            return Result.Failure(InvErrors.InventoryCount.NotFound);
        }

        var result = session.UpdateLineQuantity(request.LineId, request.CountedQuantity);
        if (result.IsFailure)
        {
            return result;
        }

        return Result.Success();
    }
}

/// <summary>Removes a count line.</summary>
public sealed class RemoveInventoryCountLineCommandHandler : IRequestHandler<RemoveInventoryCountLineCommand, Result>
{
    private readonly IInventoryCountRepository _countRepository;

    public RemoveInventoryCountLineCommandHandler(IInventoryCountRepository countRepository) => _countRepository = countRepository;

    public async Task<Result> Handle(RemoveInventoryCountLineCommand request, CancellationToken cancellationToken)
    {
        var session = await _countRepository.GetByIdWithLinesAsync(request.InventoryCountId, cancellationToken);
        if (session is null)
        {
            return Result.Failure(InvErrors.InventoryCount.NotFound);
        }

        var result = session.RemoveLine(request.LineId);
        if (result.IsFailure)
        {
            return result;
        }

        return Result.Success();
    }
}

/// <summary>Freezes expected on-hand and computes variances.</summary>
public sealed class SubmitInventoryCountCommandHandler : IRequestHandler<SubmitInventoryCountCommand, Result>
{
    private readonly IInventoryCountRepository _countRepository;
    private readonly InventoryCountOnHandResolver _onHandResolver;

    public SubmitInventoryCountCommandHandler(
        IInventoryCountRepository countRepository,
        InventoryCountOnHandResolver onHandResolver)
    {
        _countRepository = countRepository;
        _onHandResolver = onHandResolver;
    }

    public async Task<Result> Handle(SubmitInventoryCountCommand request, CancellationToken cancellationToken)
    {
        var session = await _countRepository.GetByIdWithLinesAsync(request.InventoryCountId, cancellationToken);
        if (session is null)
        {
            return Result.Failure(InvErrors.InventoryCount.NotFound);
        }

        var expectedByLine = new Dictionary<Guid, decimal>();
        foreach (var line in session.Lines)
        {
            var onHand = await _onHandResolver.GetOnHandAsync(line.ProductId, line.ProductLotId, cancellationToken);
            if (onHand.IsFailure)
            {
                return Result.Failure(onHand.Error);
            }

            expectedByLine[line.Id] = onHand.Value;
        }

        var submit = session.Submit(expectedByLine);
        if (submit.IsFailure)
        {
            return submit;
        }

        return Result.Success();
    }
}

/// <summary>Posts adjustment movements and approves the session.</summary>
public sealed class ApproveInventoryCountCommandHandler : IRequestHandler<ApproveInventoryCountCommand, Result>
{
    private readonly IInventoryCountRepository _countRepository;
    private readonly InventoryCountOnHandResolver _onHandResolver;
    private readonly StockLedgerWriter _ledgerWriter;
    private readonly ITenantContext _tenantContext;
    private readonly IAuditLogger _auditLogger;

    public ApproveInventoryCountCommandHandler(
        IInventoryCountRepository countRepository,
        InventoryCountOnHandResolver onHandResolver,
        StockLedgerWriter ledgerWriter,
        ITenantContext tenantContext,
        IAuditLogger auditLogger)
    {
        _countRepository = countRepository;
        _onHandResolver = onHandResolver;
        _ledgerWriter = ledgerWriter;
        _tenantContext = tenantContext;
        _auditLogger = auditLogger;
    }

    public async Task<Result> Handle(ApproveInventoryCountCommand request, CancellationToken cancellationToken)
    {
        var session = await _countRepository.GetByIdWithLinesAsync(request.InventoryCountId, cancellationToken);
        if (session is null)
        {
            return Result.Failure(InvErrors.InventoryCount.NotFound);
        }

        foreach (var line in session.Lines)
        {
            var onHand = await _onHandResolver.GetOnHandAsync(line.ProductId, line.ProductLotId, cancellationToken);
            if (onHand.IsFailure)
            {
                return Result.Failure(onHand.Error);
            }

            var delta = line.CountedQuantity - onHand.Value;
            if (delta == 0)
            {
                continue;
            }

            var direction = delta > 0 ? AdjustmentDirection.Increase : AdjustmentDirection.Decrease;
            var quantity = Math.Abs(delta);
            var movement = await _ledgerWriter.RegisterAsync(
                new RegisterStockLedgerRequest(
                    line.ProductId,
                    MovementType.Adjustment,
                    quantity,
                    StockMovementReasons.InventoryCount,
                    "ApproveInventoryCount",
                    line.ProductLotId,
                    direction,
                    CorrelationId: session.Id,
                    Notes: $"Inventory count {session.Code}"),
                cancellationToken);

            if (movement.IsFailure)
            {
                return Result.Failure(movement.Error);
            }

            line.MarkStockMovementApplied(movement.Value);
        }

        var approve = session.Approve();
        if (approve.IsFailure)
        {
            return approve;
        }

        await _auditLogger.LogAsync(
            _tenantContext.TenantId,
            _tenantContext.UserId,
            session.Id,
            nameof(InventoryCount),
            "Approve",
            session.Code,
            cancellationToken);

        return Result.Success();
    }
}

/// <summary>Cancels an open or submitted session.</summary>
public sealed class CancelInventoryCountCommandHandler : IRequestHandler<CancelInventoryCountCommand, Result>
{
    private readonly IInventoryCountRepository _countRepository;

    public CancelInventoryCountCommandHandler(IInventoryCountRepository countRepository) => _countRepository = countRepository;

    public async Task<Result> Handle(CancelInventoryCountCommand request, CancellationToken cancellationToken)
    {
        var session = await _countRepository.GetByIdWithLinesAsync(request.InventoryCountId, cancellationToken);
        if (session is null)
        {
            return Result.Failure(InvErrors.InventoryCount.NotFound);
        }

        var cancel = session.Cancel();
        if (cancel.IsFailure)
        {
            return cancel;
        }

        return Result.Success();
    }
}
