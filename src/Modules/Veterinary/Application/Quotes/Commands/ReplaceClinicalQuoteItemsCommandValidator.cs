using FluentValidation;

namespace Veterinary.Application.Quotes.Commands;

/// <summary>Validates quote line replace payloads.</summary>
public sealed class ReplaceClinicalQuoteItemsCommandValidator : AbstractValidator<ReplaceClinicalQuoteItemsCommand>
{
    public ReplaceClinicalQuoteItemsCommandValidator()
    {
        RuleFor(x => x.QuoteId).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Description).NotEmpty().MaximumLength(500);
            item.RuleFor(i => i.Quantity).GreaterThan(0);
            item.RuleFor(i => i.UnitPrice).GreaterThanOrEqualTo(0);
        });
    }
}

/// <summary>Validates create quote command.</summary>
public sealed class CreateClinicalQuoteCommandValidator : AbstractValidator<CreateClinicalQuoteCommand>
{
    public CreateClinicalQuoteCommandValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty();
        RuleFor(x => x.Notes).MaximumLength(2000).When(x => x.Notes is not null);
    }
}
