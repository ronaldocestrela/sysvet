using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Core.Infrastructure.Identity;
using FluentAssertions;
using Xunit;

namespace Core.Tests.Infrastructure.Options;

public class OptionsValidationTests
{
    [Fact]
    public void JwtSettings_ShouldBeValid_WithValidData()
    {
        var options = new JwtSettings
        {
            Secret = "super_secret_key_1234567890",
            Issuer = "sysvet",
            Audience = "sysvet",
            ExpiryMinutes = 60
        };

        var context = new ValidationContext(options);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(options, context, results, true);

        isValid.Should().BeTrue();
        results.Should().BeEmpty();
    }

    [Fact]
    public void JwtSettings_ShouldBeInvalid_WhenSecretIsTooShort()
    {
        var options = new JwtSettings
        {
            Secret = "short",
            Issuer = "sysvet",
            Audience = "sysvet",
            ExpiryMinutes = 60
        };

        var context = new ValidationContext(options);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(options, context, results, true);

        isValid.Should().BeFalse();
        results.Should().NotBeEmpty();
    }
}
