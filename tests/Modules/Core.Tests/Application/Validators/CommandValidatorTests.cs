using Core.Application.Auth.Commands;
using Core.Application.Authorization;
using Core.Application.Pets.Commands;
using Core.Domain.Entities;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Core.Tests.Application.Validators;

public class RegisterUserCommandValidatorTests
{
    private readonly RegisterUserCommandValidator _validator = new();

    [Fact]
    public void ValidCommand_Passes()
    {
        var result = _validator.TestValidate(new RegisterUserCommand("a@b.com", "Password1", ApplicationRoles.Admin));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void InvalidEmailRoleAndShortPassword_Fail()
    {
        var result = _validator.TestValidate(new RegisterUserCommand("bad", "short", "Nope"));
        result.ShouldHaveValidationErrorFor(x => x.Email);
        result.ShouldHaveValidationErrorFor(x => x.Password);
        result.ShouldHaveValidationErrorFor(x => x.Role);
    }
}

public class UpdatePetCommandValidatorTests
{
    private readonly UpdatePetCommandValidator _validator = new();

    [Fact]
    public void ValidCommand_Passes()
    {
        var result = _validator.TestValidate(new UpdatePetCommand(Guid.NewGuid(), "Rex", PetSpecies.Dog, "Poodle", PetSex.Male));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EmptyIdAndName_Fail()
    {
        var result = _validator.TestValidate(new UpdatePetCommand(Guid.Empty, "", PetSpecies.Dog, "Poodle", PetSex.Male));
        result.ShouldHaveValidationErrorFor(x => x.Id);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }
}
