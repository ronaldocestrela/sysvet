using Core.Domain;
using MediatR;
using System;

namespace Sales.Application.CashRegisters.Commands;

public class CloseCashRegisterCommand : IRequest<Result<bool>>
{
    public Guid CashRegisterId { get; set; }
    public decimal ActualClosingBalance { get; set; }
}
