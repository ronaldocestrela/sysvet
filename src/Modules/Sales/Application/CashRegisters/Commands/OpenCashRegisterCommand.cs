using Core.Domain;
using MediatR;
using System;

namespace Sales.Application.CashRegisters.Commands;

public class OpenCashRegisterCommand : IRequest<Result<Guid>>
{
    public decimal OpeningBalance { get; set; }
}
