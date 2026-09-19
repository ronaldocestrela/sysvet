using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Application.IntegrationEvents;
using Core.Application.Messaging;
using Core.Domain;
using Core.Domain.Auditing;
using Core.Domain.Authorization;
using Inventory.Application.Common;
using Inventory.Domain.Entities;
using Inventory.Domain.Repositories;
using Inventory.Domain.Services;
using MediatR;

namespace Inventory.Application.StockMovements.Commands;

/// <summary>Registers an auditable stock loss with a whitelisted motive.</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.StockWrite)]
public sealed record RegisterStockLossCommand(
    Guid ProductId,
    Guid? ProductLotId,
    decimal Quantity,
    string LossReasonCode,
    string? Notes,
    Guid MovementId = default,
    Guid IdempotencyKey = default) : IIdempotentCommand<Guid>;

/// <summary>Opens sealed packages into a fractional lot on the same SKU.</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.StockWrite)]
public sealed record FractionatePackageCommand(
    Guid ProductId,
    Guid SealedLotId,
    decimal PackagesToOpen,
    Guid CorrelationId = default,
    Guid IdempotencyKey = default) : IIdempotentCommand<Guid>;

/// <summary>Registers supplier return stock out and integration event.</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.StockWrite)]
public sealed record RegisterSupplierReturnCommand(
    Guid ProductId,
    Guid? ProductLotId,
    decimal Quantity,
    Guid? SupplierId,
    string? Notes,
    Guid MovementId = default,
    Guid IdempotencyKey = default) : IIdempotentCommand<Guid>;

/// <summary>Handles whitelisted loss registration.</summary>
public sealed class RegisterStockLossCommandHandler : IRequestHandler<RegisterStockLossCommand, Result<Guid>>
{
    private readonly StockLedgerWriter _ledgerWriter;

    public RegisterStockLossCommandHandler(StockLedgerWriter ledgerWriter) => _ledgerWriter = ledgerWriter;

    public Task<Result<Guid>> Handle(RegisterStockLossCommand request, CancellationToken cancellationToken)
    {
        if (!Inventory.Domain.StockLossReasons.IsValid(request.LossReasonCode))
        {
            return Task.FromResult(Result.Failure<Guid>(Inventory.Domain.ErrorCodes.StockMovement.InvalidLossReason));
        }

        var reason = Inventory.Domain.StockLossReasons.ToMovementReason(request.LossReasonCode);
        return _ledgerWriter.RegisterAsync(
            new RegisterStockLedgerRequest(
                request.ProductId,
                MovementType.Out,
                request.Quantity,
                reason,
                "RegisterLoss",
                request.ProductLotId,
                Notes: request.Notes,
                MovementId: request.MovementId),
            cancellationToken);
    }
}

/// <summary>Handles package fractionation via transfer into fractional lot.</summary>
public sealed class FractionatePackageCommandHandler : IRequestHandler<FractionatePackageCommand, Result<Guid>>
{
    private readonly IProductRepository _productRepository;
    private readonly IProductLotRepository _lotRepository;
    private readonly TransferStockCommandHandler _transferHandler;

    public FractionatePackageCommandHandler(
        IProductRepository productRepository,
        IProductLotRepository lotRepository,
        TransferStockCommandHandler transferHandler)
    {
        _productRepository = productRepository;
        _lotRepository = lotRepository;
        _transferHandler = transferHandler;
    }

    public async Task<Result<Guid>> Handle(FractionatePackageCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null || !product.IsActive)
        {
            return Result.Failure<Guid>(Inventory.Domain.ErrorCodes.Product.NotFound);
        }

        var sealedLot = await _lotRepository.GetByIdAsync(request.SealedLotId, cancellationToken);
        if (sealedLot is null)
        {
            return Result.Failure<Guid>(Inventory.Domain.ErrorCodes.ProductLot.NotFound);
        }

        var planResult = PackageFractionationService.Plan(product, sealedLot, request.PackagesToOpen);
        if (planResult.IsFailure)
        {
            return Result.Failure<Guid>(planResult.Error);
        }

        var plan = planResult.Value;
        var destination = await _lotRepository.GetByProductAndLotNumberAsync(product.Id, plan.TargetLotNumber, cancellationToken);
        if (destination is null)
        {
            var create = ProductLot.Create(
                product.Id,
                plan.TargetLotNumber,
                plan.ExpirationDate,
                plan.UnitCost,
                0m,
                isFractional: true);
            if (create.IsFailure)
            {
                return Result.Failure<Guid>(create.Error);
            }

            destination = create.Value;
            _lotRepository.Add(destination);
        }

        var correlationId = request.CorrelationId == Guid.Empty ? Guid.NewGuid() : request.CorrelationId;
        return await _transferHandler.Handle(
            new TransferStockCommand(
                product.Id,
                sealedLot.Id,
                destination.Id,
                plan.QuantityToTransfer,
                Inventory.Domain.StockMovementReasons.Fractionation,
                correlationId),
            cancellationToken);
    }
}

/// <summary>Handles supplier return out and publishes integration event.</summary>
public sealed class RegisterSupplierReturnCommandHandler : IRequestHandler<RegisterSupplierReturnCommand, Result<Guid>>
{
    private readonly IProductRepository _productRepository;
    private readonly IProductLotRepository _lotRepository;
    private readonly ISupplierRepository _supplierRepository;
    private readonly StockLedgerWriter _ledgerWriter;
    private readonly IPublisher _publisher;

    public RegisterSupplierReturnCommandHandler(
        IProductRepository productRepository,
        IProductLotRepository lotRepository,
        ISupplierRepository supplierRepository,
        StockLedgerWriter ledgerWriter,
        IPublisher publisher)
    {
        _productRepository = productRepository;
        _lotRepository = lotRepository;
        _supplierRepository = supplierRepository;
        _ledgerWriter = ledgerWriter;
        _publisher = publisher;
    }

    public async Task<Result<Guid>> Handle(RegisterSupplierReturnCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null || !product.IsActive)
        {
            return Result.Failure<Guid>(Inventory.Domain.ErrorCodes.Product.NotFound);
        }

        var supplierId = request.SupplierId ?? product.SupplierId;
        if (supplierId is null || supplierId == Guid.Empty)
        {
            return Result.Failure<Guid>(Inventory.Domain.ErrorCodes.StockMovement.SupplierRequired);
        }

        var supplier = await _supplierRepository.GetByIdAsync(supplierId.Value, cancellationToken);
        if (supplier is null || !supplier.IsActive)
        {
            return Result.Failure<Guid>(Inventory.Domain.ErrorCodes.Supplier.NotFound);
        }

        decimal unitCost = product.AverageCost;
        if (request.ProductLotId is Guid lotId)
        {
            var lot = await _lotRepository.GetByIdAsync(lotId, cancellationToken);
            if (lot is not null)
            {
                unitCost = lot.UnitCost;
            }
        }

        var register = await _ledgerWriter.RegisterAsync(
            new RegisterStockLedgerRequest(
                request.ProductId,
                MovementType.Out,
                request.Quantity,
                Inventory.Domain.StockMovementReasons.SupplierReturn,
                "SupplierReturn",
                request.ProductLotId,
                Notes: request.Notes,
                MovementId: request.MovementId,
                SupplierId: supplierId),
            cancellationToken);

        if (register.IsFailure)
        {
            return register;
        }

        await _publisher.Publish(
            new SupplierReturnRegisteredEvent(
                register.Value,
                request.ProductId,
                supplierId.Value,
                request.Quantity,
                request.Quantity * unitCost),
            cancellationToken);

        return register;
    }
}
