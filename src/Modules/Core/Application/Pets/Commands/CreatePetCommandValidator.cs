using FluentValidation;

namespace Core.Application.Pets.Commands;

public class CreatePetCommandValidator : AbstractValidator<CreatePetCommand>
{
    public CreatePetCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("O nome do pet é obrigatório.");
        RuleFor(x => x.Species).IsInEnum().WithMessage("A espécie fornecida é inválida.");
        RuleFor(x => x.Sex).IsInEnum().WithMessage("O sexo fornecido é inválido.");
        RuleFor(x => x.TutorId).NotEmpty().WithMessage("O TutorId é obrigatório.");
    }
}
