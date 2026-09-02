using Core.Domain;
using MediatR;
using System;
using System.Collections.Generic;

namespace Sales.Application.Orders.Commands;

public class CreateOrderCommand : IRequest<Result<Guid>>
{
    public Guid CashRegisterId { get; set; }
    public List<CreateOrderItemDto> Items { get; set; } = new();
}

public class CreateOrderItemDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
