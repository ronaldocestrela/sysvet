using Core.Domain;
using MediatR;
using System;

namespace Sales.Application.Orders.Commands;

public class PayOrderCommand : IRequest<Result<bool>>
{
    public Guid OrderId { get; set; }
}
