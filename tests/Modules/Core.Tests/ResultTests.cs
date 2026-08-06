using Core.Domain;

namespace Core.Tests;

public class ResultTests
{
    [Fact]
    public void Success_ShouldReturnIsSuccessTrue_AndNoErrors()
    {
        // Act
        var result = Result.Success();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(Error.None, result.Error);
    }

    [Fact]
    public void Failure_ShouldReturnIsSuccessFalse_AndCorrectError()
    {
        // Arrange
        var error = new Error("Test.Error", "This is a test error.");

        // Act
        var result = Result.Failure(error);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void SuccessWithValue_ShouldReturnIsSuccessTrue_AndCorrectValue()
    {
        // Arrange
        var expectedValue = "Hello SysVet";

        // Act
        var result = Result.Success(expectedValue);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedValue, result.Value);
    }

    [Fact]
    public void FailureWithValue_ShouldThrowException_WhenAccessingValue()
    {
        // Arrange
        var error = new Error("Test.Error", "Error with value type.");

        // Act
        var result = Result.Failure<string>(error);

        // Assert
        Assert.Throws<InvalidOperationException>(() => result.Value);
    }
}
