using FluentValidation;

namespace Core.Application.Auth.Commands;

/// <summary>
/// Ensures a refresh token string is present.
/// </summary>
public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
