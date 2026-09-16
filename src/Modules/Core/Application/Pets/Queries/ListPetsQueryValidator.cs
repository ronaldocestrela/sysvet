using FluentValidation;

namespace Core.Application.Pets.Queries;

/// <summary>
/// Validates pagination parameters for pet list queries.
/// </summary>
public class ListPetsQueryValidator : AbstractValidator<ListPetsQuery>
{
    /// <summary>
    /// Initializes validation rules.
    /// </summary>
    public ListPetsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
