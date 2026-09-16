using Core.Application.Authorization;
using FluentValidation;

namespace Core.Application.Auth.Commands;

/// <summary>
/// Validates dev registration input including allowed roles.
/// </summary>
public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.Role)
            .NotEmpty()
            .Must(role => ApplicationRoles.All.Contains(role))
            .WithMessage("Role must be one of: Admin, Veterinarian, Receptionist, Cashier.");
    }
}
