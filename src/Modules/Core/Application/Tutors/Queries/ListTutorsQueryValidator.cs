using FluentValidation;

namespace Core.Application.Tutors.Queries;

/// <summary>
/// Validates pagination parameters for tutor list queries.
/// </summary>
public class ListTutorsQueryValidator : AbstractValidator<ListTutorsQuery>
{
    /// <summary>
    /// Initializes validation rules.
    /// </summary>
    public ListTutorsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
