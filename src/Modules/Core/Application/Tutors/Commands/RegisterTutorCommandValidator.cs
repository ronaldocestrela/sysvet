using FluentValidation;

namespace Core.Application.Tutors.Commands;

public class RegisterTutorCommandValidator : AbstractValidator<RegisterTutorCommand>
{
    public RegisterTutorCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome é obrigatório.")
            .MinimumLength(2).WithMessage("O nome deve ter pelo menos 2 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("O e-mail é obrigatório.")
            .EmailAddress().WithMessage("O e-mail fornecido não é válido.");

        RuleFor(x => x.Cpf)
            .NotEmpty().WithMessage("O CPF é obrigatório.")
            .Length(11).WithMessage("O CPF deve conter 11 dígitos.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("O telefone é obrigatório.");
    }
}
