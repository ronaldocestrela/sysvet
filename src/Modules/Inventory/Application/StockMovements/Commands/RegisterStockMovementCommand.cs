using System;
using Core.Domain;
using Inventory.Domain.Entities;
using MediatR;

namespace Inventory.Application.StockMovements.Commands;

public record RegisterStockMovementCommand(
    Guid ProductId,
    MovementType Type,
    decimal Quantity,
    string? BatchNumber,
    DateTimeOffset? ExpirationDate,
    string Reason
) : IRequest<Result<Guid>>;
