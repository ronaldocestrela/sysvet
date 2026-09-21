using FluentValidation;
using Sales.Application.CashRegisters.Commands;

namespace Sales.Application.CashRegisters.Validators;

/// <summary>FluentValidation rules for cash register commands.</summary>
public sealed class RecordCashMovementCommandValidator : AbstractValidator<RecordCashMovementCommand>
{
    public RecordCashMovementCommandValidator()
    {
        RuleFor(x => x.CashRegisterId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}

public sealed class OpenCashRegisterCommandValidator : AbstractValidator<OpenCashRegisterCommand>
{
    public OpenCashRegisterCommandValidator()
    {
        When(x => x.CashRegisterId.HasValue, () => RuleFor(x => x.CashRegisterId!.Value).NotEmpty());
        RuleFor(x => x.OpeningBalance).GreaterThanOrEqualTo(0);
    }
}

/// <summary>FluentValidation rules for closing the cash register.</summary>
public sealed class CloseCashRegisterCommandValidator : AbstractValidator<CloseCashRegisterCommand>
{
    public CloseCashRegisterCommandValidator()
    {
        RuleFor(x => x.CashRegisterId).NotEmpty();
        RuleFor(x => x.ActualClosingBalance).GreaterThanOrEqualTo(0);
    }
}
