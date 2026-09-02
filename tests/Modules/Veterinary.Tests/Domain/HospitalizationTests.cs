using System;
using FluentAssertions;
using Xunit;
using Veterinary.Domain.Entities;

namespace Veterinary.Tests.Domain;

public class HospitalizationTests
{
    [Fact]
    public void Admit_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var petId = Guid.NewGuid();
        var veterinarianId = Guid.NewGuid();
        var reason = "Severe dehydration";

        // Act
        var result = Hospitalization.Admit(Guid.NewGuid(), petId, veterinarianId, reason);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.PetId.Should().Be(petId);
        result.Value.Status.Should().Be(HospitalizationStatus.Admitted);
        result.Value.DischargedAt.Should().BeNull();
    }

    [Fact]
    public void Discharge_WhenAdmitted_SetsDischargeDateAndStatus()
    {
        // Arrange
        var hosp = Hospitalization.Admit(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Dehydration").Value;

        // Act
        var result = hosp.Discharge();

        // Assert
        result.IsSuccess.Should().BeTrue();
        hosp.Status.Should().Be(HospitalizationStatus.Discharged);
        hosp.DischargedAt.Should().NotBeNull();
    }

    [Fact]
    public void Discharge_WhenAlreadyDischarged_ReturnsFailure()
    {
        // Arrange
        var hosp = Hospitalization.Admit(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Dehydration").Value;
        hosp.Discharge();

        // Act
        var result = hosp.Discharge();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Hospitalization.AlreadyDischarged");
    }

    [Fact]
    public void ExecutePrescription_WhenAdmitted_AddsExecution()
    {
        // Arrange
        var hosp = Hospitalization.Admit(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Dehydration").Value;

        // Act
        var result = hosp.ExecutePrescription("Dipyrone", "500mg", "Patient responded well", Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeTrue();
        hosp.PrescriptionExecutions.Should().HaveCount(1);
    }

    [Fact]
    public void ExecutePrescription_WhenDischarged_ReturnsFailure()
    {
        // Arrange
        var hosp = Hospitalization.Admit(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Dehydration").Value;
        hosp.Discharge();

        // Act
        var result = hosp.ExecutePrescription("Dipyrone", "500mg", "", Guid.NewGuid());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Hospitalization.Discharged");
    }
}
