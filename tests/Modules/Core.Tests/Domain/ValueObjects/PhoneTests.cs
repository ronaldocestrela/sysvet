using FluentAssertions;
using Core.Domain.ValueObjects;

namespace Core.Tests.Domain.ValueObjects;

public class PhoneTests
{
    [Theory]
    [InlineData("11999998888")]
    [InlineData("(11) 99999-8888")]
    [InlineData("+5511999998888")]
    public void Create_ShouldReturnSuccess_WhenPhoneIsValid(string rawPhone)
    {
        // Act
        var result = Phone.Create(rawPhone);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Number.Should().Be("11999998888");
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("abcdefghijk")]
    public void Create_ShouldReturnFailure_WhenPhoneIsInvalid(string invalidPhone)
    {
        // Act
        var result = Phone.Create(invalidPhone);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Phone.InvalidFormat");
    }
}
