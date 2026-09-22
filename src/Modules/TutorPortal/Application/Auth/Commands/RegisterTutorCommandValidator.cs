using FluentValidation;

namespace TutorPortal.Application.Auth.Commands;

/// <summary>
/// Validates tutor self-registration input.
/// </summary>
public sealed class RegisterTutorCommandValidator : AbstractValidator<RegisterTutorCommand>
{
    public RegisterTutorCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Cpf).NotEmpty().MinimumLength(11);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}
