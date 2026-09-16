using FluentValidation;

namespace Core.Application.Auth.Commands;

/// <summary>
/// Validates login request shape before credential checks.
/// </summary>
public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}
