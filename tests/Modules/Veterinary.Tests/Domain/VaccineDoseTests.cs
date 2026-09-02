using FluentAssertions;
using Xunit;
using Veterinary.Domain.Entities;
using System;

namespace Veterinary.Domain.Tests;

public class VaccineDoseTests
{
    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var name = "Rabies";
        var batchNumber = "B12345";
        var appliedAt = DateTimeOffset.UtcNow;
        var nextDueDate = appliedAt.AddYears(1);

        // Act
        var result = VaccineDose.Create(
            Guid.NewGuid(), 
            petId, 
            name, 
            batchNumber, 
            appliedAt, 
            nextDueDate);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.PetId.Should().Be(petId);
        result.Value.Name.Should().Be(name);
        result.Value.BatchNumber.Should().Be(batchNumber);
    }

    [Fact]
    public void Create_WithEmptyName_ReturnsFailure()
    {
        // Act
        var result = VaccineDose.Create(
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            string.Empty, 
            "B123", 
            DateTimeOffset.UtcNow, 
            null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("VaccineDose.InvalidName");
    }

    [Fact]
    public void Create_WithFutureAppliedAt_ReturnsFailure()
    {
        // Act
        var result = VaccineDose.Create(
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            "Rabies", 
            "B123", 
            DateTimeOffset.UtcNow.AddDays(1), // Applied in the future
            null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("VaccineDose.FutureApplicationDate");
    }
}
