using Core.Domain;
using FluentAssertions;
using Xunit;

namespace Core.Tests.Domain;

public class ErrorCodeTests
{
    [Fact]
    public void ErrorCode_Should_HavePredefinedCoreErrors()
    {
        // Assert
        ErrorCodes.Tutor.NotFound.Code.Should().Be("Tutor.NotFound");
        ErrorCodes.Tutor.NotFound.Message.Should().NotBeNullOrEmpty();

        ErrorCodes.Pet.NotFound.Code.Should().Be("Pet.NotFound");
        ErrorCodes.Pet.NotFound.Message.Should().NotBeNullOrEmpty();
    }
}
