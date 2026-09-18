using FluentValidation;

namespace Veterinary.Application.Vaccines.Commands;

/// <summary>Validates vaccine dose registration input.</summary>
public sealed class RegisterVaccineDoseCommandValidator : AbstractValidator<RegisterVaccineDoseCommand>
{
    public RegisterVaccineDoseCommandValidator()
    {
        RuleFor(c => c.PetId).NotEmpty();
        RuleFor(c => c.Name).MaximumLength(100);
        RuleFor(c => c.BatchNumber).MaximumLength(50);
    }
}
