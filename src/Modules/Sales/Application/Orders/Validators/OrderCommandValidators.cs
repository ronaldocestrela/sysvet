using FluentValidation;
using Sales.Application.Orders.Commands;
using Sales.Domain.Enums;

namespace Sales.Application.Orders.Validators;

public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        When(x => x.OrderId.HasValue, () => RuleFor(x => x.OrderId!.Value).NotEmpty());
        RuleFor(x => x.CashRegisterId).NotEmpty();
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Quantity).GreaterThan(0);
            item.RuleFor(i => i.UnitPrice).GreaterThanOrEqualTo(0);
            item.When(i => i.Kind == OrderItemKind.Product, () =>
            {
                item.RuleFor(i => i.ProductId).NotEmpty();
                item.RuleFor(i => i.ProductName).NotEmpty();
            });
            item.When(i => i.Kind == OrderItemKind.Service, () =>
            {
                item.RuleFor(i => i.ProductName).NotEmpty();
            });
        });
    }
}

public sealed class PayOrderCommandValidator : AbstractValidator<PayOrderCommand>
{
    public PayOrderCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.Payments).NotEmpty();
        RuleForEach(x => x.Payments).ChildRules(p =>
        {
            p.RuleFor(x => x.Amount).GreaterThan(0);
        });
    }
}
