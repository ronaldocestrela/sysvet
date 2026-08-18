using FluentValidation;

namespace Core.Application.Pets.Commands;

public class UpdatePetCommandValidator : AbstractValidator<UpdatePetCommand>
{
    public UpdatePetCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("O ID do pet é obrigatório.");
        RuleFor(x => x.Name).NotEmpty().WithMessage("O nome do pet é obrigatório.");
        RuleFor(x => x.Species).IsInEnum().WithMessage("A espécie fornecida é inválida.");
        RuleFor(x => x.Sex).IsInEnum().WithMessage("O sexo fornecido é inválido.");
    }
}
