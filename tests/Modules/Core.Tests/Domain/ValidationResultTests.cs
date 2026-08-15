using Core.Domain;
using FluentAssertions;
using Xunit;

namespace Core.Tests.Domain;

public class ValidationResultTests
{
    [Fact]
    public void Constructor_Should_CreateValidationResult_WithErrors()
    {
        // Arrange
        var errors = new[]
        {
            new Error("Property1", "Error 1"),
            new Error("Property2", "Error 2")
        };

        // Act
        var validationResult = ValidationResult.WithErrors(errors);

        // Assert
        validationResult.IsFailure.Should().BeTrue();
        validationResult.Error.Code.Should().Be("Validation.Error");
        validationResult.ValidationErrors.Should().BeEquivalentTo(errors);
    }
}
