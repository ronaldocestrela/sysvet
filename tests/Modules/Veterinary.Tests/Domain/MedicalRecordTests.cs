using FluentAssertions;
using Xunit;
using Veterinary.Domain.Entities;
using System;

namespace Veterinary.Domain.Tests;

public class MedicalRecordTests
{
    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var veterinarianId = Guid.NewGuid();
        var tutorId = Guid.NewGuid();
        var petId = Guid.NewGuid();

        // Act
        var result = MedicalRecord.Create(
            Guid.NewGuid(), 
            appointmentId, 
            veterinarianId, 
            tutorId, 
            petId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AppointmentId.Should().Be(appointmentId);
        result.Value.Status.Should().Be(MedicalRecordStatus.Draft);
    }

    [Fact]
    public void AppendNotes_WhenDraft_UpdatesNotes()
    {
        // Arrange
        var record = MedicalRecord.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()).Value;

        // Act
        var result = record.AppendDiagnosis("Fever and lethargy");

        // Assert
        result.IsSuccess.Should().BeTrue();
        record.Diagnosis.Should().Be("Fever and lethargy");
    }

    [Fact]
    public void AppendDiagnosis_WhenFinalized_ReturnsFailure()
    {
        // Arrange
        var record = MedicalRecord.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()).Value;
        record.FinalizeRecord();

        // Act
        var result = record.AppendDiagnosis("Fever and lethargy");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("MedicalRecord.Finalized");
    }

    [Fact]
    public void FinalizeRecord_WhenAlreadyFinalized_ReturnsFailure()
    {
        // Arrange
        var record = MedicalRecord.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()).Value;
        record.FinalizeRecord();

        // Act
        var result = record.FinalizeRecord();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("MedicalRecord.AlreadyFinalized");
    }
}
