using FluentValidation;

namespace TutorPortal.Application.Auth.Commands;

/// <summary>
/// Validates tutor portal login input.
/// </summary>
public sealed class TutorLoginCommandValidator : AbstractValidator<TutorLoginCommand>
{
    public TutorLoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}
