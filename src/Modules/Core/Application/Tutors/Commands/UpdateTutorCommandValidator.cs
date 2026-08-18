using FluentValidation;

namespace Core.Application.Tutors.Commands;

public class UpdateTutorCommandValidator : AbstractValidator<UpdateTutorCommand>
{
    public UpdateTutorCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("O ID do tutor é obrigatório.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome é obrigatório.")
            .MinimumLength(2).WithMessage("O nome deve ter pelo menos 2 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("O e-mail é obrigatório.")
            .EmailAddress().WithMessage("O e-mail fornecido não é válido.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("O telefone é obrigatório.");
    }
}
