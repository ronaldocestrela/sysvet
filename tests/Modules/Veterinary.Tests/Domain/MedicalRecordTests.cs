using FluentAssertions;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Enums;
using Veterinary.Domain.ValueObjects;

namespace Veterinary.Domain.Tests;

public class MedicalRecordTests
{
    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        var appointmentId = Guid.NewGuid();
        var result = MedicalRecord.Create(
            Guid.NewGuid(),
            appointmentId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.AppointmentId.Should().Be(appointmentId);
        result.Value.Status.Should().Be(MedicalRecordStatus.Draft);
    }

    [Fact]
    public void AppendNotes_WhenDraft_UpdatesNotes()
    {
        var record = MedicalRecord.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()).Value;

        var result = record.AppendDiagnosis("Fever and lethargy");

        result.IsSuccess.Should().BeTrue();
        record.Diagnosis.Should().Be("Fever and lethargy");
    }

    [Fact]
    public void AppendDiagnosis_WhenFinalized_ReturnsFailure()
    {
        var record = MedicalRecord.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()).Value;
        record.FinalizeRecord();

        var result = record.AppendDiagnosis("Fever and lethargy");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("MedicalRecord.Finalized");
    }

    [Fact]
    public void FinalizeRecord_WhenAlreadyFinalized_ReturnsFailure()
    {
        var record = MedicalRecord.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()).Value;
        record.FinalizeRecord();

        var result = record.FinalizeRecord();

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("MedicalRecord.AlreadyFinalized");
    }

    [Fact]
    public void SetAnamnesis_WhenDraft_UpdatesAnamnesis()
    {
        var record = MedicalRecord.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()).Value;

        var result = record.SetAnamnesis("  Vomiting since yesterday  ");

        result.IsSuccess.Should().BeTrue();
        record.Anamnesis.Should().Be("Vomiting since yesterday");
    }

    [Fact]
    public void RecordVitalSigns_WithInvalidWeight_ReturnsFailure()
    {
        var record = MedicalRecord.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()).Value;
        var vitals = VitalSigns.Create(-1m, 38.5m, 90, 20, "pink", "<2s", DateTimeOffset.UtcNow);

        vitals.IsFailure.Should().BeTrue();
        vitals.Error.Code.Should().Be("MedicalRecord.InvalidVitals");
    }

    [Fact]
    public void RecordVitalSigns_WhenDraft_StoresSnapshot()
    {
        var record = MedicalRecord.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()).Value;
        var vitals = VitalSigns.Create(12.5m, 38.5m, 90, 20, "pink", "<2s", DateTimeOffset.UtcNow).Value;

        var result = record.RecordVitalSigns(vitals);

        result.IsSuccess.Should().BeTrue();
        record.VitalSigns!.WeightKg.Should().Be(12.5m);
    }

    [Fact]
    public void AddEvolutionNote_WithEmptyText_ReturnsFailure()
    {
        var record = MedicalRecord.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()).Value;

        var result = record.AddEvolutionNote(Guid.NewGuid(), Guid.NewGuid(), "   ", DateTimeOffset.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("MedicalRecord.EmptyEvolution");
    }

    [Fact]
    public void AddEvolutionNote_WhenDraft_AppendsNote()
    {
        var record = MedicalRecord.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()).Value;
        var authorId = Guid.NewGuid();
        var noteId = Guid.NewGuid();

        var result = record.AddEvolutionNote(noteId, authorId, "Improved appetite", DateTimeOffset.UtcNow);

        result.IsSuccess.Should().BeTrue();
        record.EvolutionNotes.Should().ContainSingle(n => n.Id == noteId && n.AuthorId == authorId);
    }

    [Fact]
    public void ApplySyncSnapshot_WhenLocalFinalized_DoesNotReopenToDraft()
    {
        var record = MedicalRecord.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()).Value;
        record.FinalizeRecord();

        record.ApplySyncSnapshot(
            "remote anamnesis",
            "remote dx",
            "remote plan",
            MedicalRecordStatus.Draft,
            null,
            DateTimeOffset.UtcNow,
            Array.Empty<(Guid, Guid, string, DateTimeOffset)>());

        record.Status.Should().Be(MedicalRecordStatus.Finalized);
        record.Anamnesis.Should().Be("remote anamnesis");
    }

    [Fact]
    public void Appointment_IsEligibleForMedicalRecord_OnlyInProgressOrCompleted()
    {
        var appointment = Appointment.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(1), 30, "Check").Value;
        appointment.IsEligibleForMedicalRecord().Should().BeFalse();

        appointment.Confirm().IsSuccess.Should().BeTrue();
        appointment.IsEligibleForMedicalRecord().Should().BeFalse();

        appointment.Start().IsSuccess.Should().BeTrue();
        appointment.IsEligibleForMedicalRecord().Should().BeTrue();

        appointment.Complete().IsSuccess.Should().BeTrue();
        appointment.IsEligibleForMedicalRecord().Should().BeTrue();
    }
}
