using System;
using Core.Domain;
using MediatR;

namespace Inventory.Application.Products.Commands;

public record RegisterProductCommand(
    string Name,
    string Description,
    string Barcode,
    string UnitOfMeasure,
    decimal ReorderLevel
) : IRequest<Result<Guid>>;
