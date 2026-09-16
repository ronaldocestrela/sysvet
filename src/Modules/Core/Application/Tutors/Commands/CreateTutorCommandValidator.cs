using FluentValidation;

namespace Core.Application.Tutors.Commands;

/// <summary>
/// FluentValidation rules for <see cref="CreateTutorCommand"/>.
/// </summary>
public class CreateTutorCommandValidator : AbstractValidator<CreateTutorCommand>
{
    /// <summary>
    /// Initializes validation rules for tutor creation.
    /// </summary>
    public CreateTutorCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome é obrigatório.")
            .MinimumLength(2).WithMessage("O nome deve ter pelo menos 2 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("O e-mail é obrigatório.")
            .EmailAddress().WithMessage("O e-mail fornecido não é válido.");

        RuleFor(x => x.Cpf)
            .NotEmpty().WithMessage("O CPF é obrigatório.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("O telefone é obrigatório.");
    }
}
