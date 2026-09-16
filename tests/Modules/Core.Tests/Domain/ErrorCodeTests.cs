using Core.Domain;
using FluentAssertions;
using Xunit;

namespace Core.Tests.Domain;

public class ErrorCodeTests
{
    [Fact]
    public void ErrorCode_Should_HavePredefinedCoreErrors()
    {
        ErrorCodes.Tutor.NotFound.Code.Should().Be("Tutor.NotFound");
        ErrorCodes.Tutor.NotFound.Message.Should().NotBeNullOrEmpty();
        ErrorCodes.Tutor.InvalidName.Code.Should().Be("Tutor.InvalidName");

        ErrorCodes.Pet.NotFound.Code.Should().Be("Pet.NotFound");
        ErrorCodes.Pet.NotFound.Message.Should().NotBeNullOrEmpty();

        ErrorCodes.Authorization.Unauthorized.Code.Should().Be("Authorization.Unauthorized");
        ErrorCodes.Authorization.Forbidden.Code.Should().Be("Authorization.Forbidden");

        ErrorCodes.Validation.Error.Code.Should().Be("Validation.Error");
        ErrorCodes.Cpf.InvalidFormat.Code.Should().Be("Cpf.InvalidFormat");
    }
}
