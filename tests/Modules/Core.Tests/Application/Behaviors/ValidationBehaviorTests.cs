using Core.Application.Behaviors;
using Core.Domain;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using NSubstitute;
using Xunit;

namespace Core.Tests.Application.Behaviors;

public class ValidationBehaviorTests
{
    public class TestCommand : IRequest<Result<string>>
    {
        public string Name { get; set; } = string.Empty;
    }

    public class TestCommandValidator : AbstractValidator<TestCommand>
    {
        public TestCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
        }
    }

    [Fact]
    public async Task Handle_Should_CallNext_When_NoValidatorsAreProvided()
    {
        // Arrange
        var request = new TestCommand { Name = "Valid Name" };
        var nextDelegate = Substitute.For<RequestHandlerDelegate<Result<string>>>();
        nextDelegate.Invoke().Returns(Task.FromResult(Result.Success("Success")));

        var behavior = new ValidationBehavior<TestCommand, Result<string>>(Enumerable.Empty<IValidator<TestCommand>>());

        // Act
        var result = await behavior.Handle(request, nextDelegate, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await nextDelegate.Received(1).Invoke();
    }

    [Fact]
    public async Task Handle_Should_CallNext_When_ValidationPasses()
    {
        // Arrange
        var request = new TestCommand { Name = "Valid Name" };
        var nextDelegate = Substitute.For<RequestHandlerDelegate<Result<string>>>();
        nextDelegate.Invoke().Returns(Task.FromResult(Result.Success("Success")));

        var validators = new[] { new TestCommandValidator() };
        var behavior = new ValidationBehavior<TestCommand, Result<string>>(validators);

        // Act
        var result = await behavior.Handle(request, nextDelegate, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await nextDelegate.Received(1).Invoke();
    }

    [Fact]
    public async Task Handle_Should_ReturnValidationResult_When_ValidationFails()
    {
        // Arrange
        var request = new TestCommand { Name = "" }; // Invalid name
        var nextDelegate = Substitute.For<RequestHandlerDelegate<Result<string>>>();

        var validators = new[] { new TestCommandValidator() };
        var behavior = new ValidationBehavior<TestCommand, Result<string>>(validators);

        // Act
        var result = await behavior.Handle(request, nextDelegate, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        
        // Ensure that next() was not called because of validation errors
        await nextDelegate.DidNotReceive().Invoke();

        var validationResult = result.Should().BeOfType<ValidationResult<string>>().Subject;
        validationResult.ValidationErrors.Should().ContainSingle()
            .Which.Message.Should().Be("Name is required.");
    }
}
