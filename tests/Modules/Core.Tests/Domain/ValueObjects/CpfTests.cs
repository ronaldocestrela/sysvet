using FluentAssertions;
using Core.Domain.ValueObjects;

namespace Core.Tests.Domain.ValueObjects;

public class CpfTests
{
    [Theory]
    [InlineData("12345678909")]
    [InlineData("123.456.789-09")]
    public void Create_ShouldReturnSuccess_WhenCpfIsValid(string rawCpf)
    {
        // Act
        var result = Cpf.Create(rawCpf);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Number.Should().Be("12345678909");
    }

    [Theory]
    [InlineData("")]
    [InlineData("1234")]
    [InlineData("11111111111")] // Invalid CPF checksum pattern
    [InlineData("abcdefghijk")]
    public void Create_ShouldReturnFailure_WhenCpfIsInvalid(string invalidCpf)
    {
        // Act
        var result = Cpf.Create(invalidCpf);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Cpf.InvalidFormat");
    }
}
