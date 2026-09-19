using FluentAssertions;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Enums;
using Veterinary.Domain.Services;

namespace Veterinary.Tests.Domain;

public class HospitalizationTests
{
    private static Hospitalization CreateAdmitted()
    {
        return Hospitalization.Admit(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Dehydration",
            DateTimeOffset.UtcNow).Value;
    }

    [Fact]
    public void Admit_WithValidData_ReturnsSuccess()
    {
        var petId = Guid.NewGuid();
        var veterinarianId = Guid.NewGuid();
        var bedId = Guid.NewGuid();

        var result = Hospitalization.Admit(Guid.NewGuid(), petId, veterinarianId, bedId, "Severe dehydration", DateTimeOffset.UtcNow);

        result.IsSuccess.Should().BeTrue();
        result.Value.PetId.Should().Be(petId);
        result.Value.BedId.Should().Be(bedId);
        result.Value.Status.Should().Be(HospitalizationStatus.Admitted);
        result.Value.DischargedAt.Should().BeNull();
    }

    [Fact]
    public void Discharge_WhenAdmitted_CancelsPendingAdministrations()
    {
        var hosp = CreateAdmitted();
        var orderId = Guid.NewGuid();
        hosp.AddMedicationOrder(
            orderId,
            "Dipyrone",
            "500mg",
            "IV",
            [new TimeOnly(8, 0)],
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow));

        var pending = hosp.Administrations.First(a => a.Status == MedicationAdministrationStatus.Pending);

        var result = hosp.Discharge(DateTimeOffset.UtcNow);

        result.IsSuccess.Should().BeTrue();
        hosp.Status.Should().Be(HospitalizationStatus.Discharged);
        pending.Status.Should().Be(MedicationAdministrationStatus.Cancelled);
    }

    [Fact]
    public void AddMedicationOrder_ExpandsAdministrationSlots()
    {
        var hosp = CreateAdmitted();
        var starts = new DateOnly(2026, 9, 18);
        var ends = new DateOnly(2026, 9, 19);

        var result = hosp.AddMedicationOrder(
            Guid.NewGuid(),
            "Ondansetron",
            "4mg",
            "IV",
            [new TimeOnly(8, 0), new TimeOnly(20, 0)],
            starts,
            ends);

        result.IsSuccess.Should().BeTrue();
        var expected = MedicationSchedule.ExpandOccurrences(starts, ends, [new TimeOnly(8, 0), new TimeOnly(20, 0)]);
        hosp.Administrations.Should().HaveCount(expected.Count);
    }

    [Fact]
    public void AdministerMedication_WhenPending_SetsAdministered()
    {
        var hosp = CreateAdmitted();
        hosp.AddMedicationOrder(
            Guid.NewGuid(),
            "Dipyrone",
            "500mg",
            "IV",
            [new TimeOnly(8, 0)],
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow));

        var adminId = hosp.Administrations.First().Id;
        var actor = Guid.NewGuid();
        var result = hosp.AdministerMedication(adminId, actor, "OK", DateTimeOffset.UtcNow);

        result.IsSuccess.Should().BeTrue();
        hosp.Administrations.First(a => a.Id == adminId).Status.Should().Be(MedicationAdministrationStatus.Administered);
        hosp.Administrations.First(a => a.Id == adminId).ActorId.Should().Be(actor);
    }

    [Fact]
    public void AdministerMedication_WhenDischarged_ReturnsFailure()
    {
        var hosp = CreateAdmitted();
        hosp.Discharge(DateTimeOffset.UtcNow);

        var result = hosp.AddMedicationOrder(
            Guid.NewGuid(),
            "Dipyrone",
            "500mg",
            "IV",
            [new TimeOnly(8, 0)],
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Hospitalization.Discharged");
    }

    [Fact]
    public void TransferBed_WhenAdmitted_UpdatesBedId()
    {
        var hosp = CreateAdmitted();
        var newBed = Guid.NewGuid();

        var result = hosp.TransferBed(newBed);

        result.IsSuccess.Should().BeTrue();
        hosp.BedId.Should().Be(newBed);
    }

    [Fact]
    public void AddProgressNote_WhenAdmitted_AppendsNote()
    {
        var hosp = CreateAdmitted();
        var result = hosp.AddProgressNote(Guid.NewGuid(), Guid.NewGuid(), "Stable vitals", DateTimeOffset.UtcNow);

        result.IsSuccess.Should().BeTrue();
        hosp.ProgressNotes.Should().HaveCount(1);
    }

    [Fact]
    public void AddProcedure_WhenAdmitted_AppendsProcedure()
    {
        var hosp = CreateAdmitted();
        var result = hosp.AddProcedure(Guid.NewGuid(), "Fluid therapy", Guid.NewGuid(), DateTimeOffset.UtcNow, null);

        result.IsSuccess.Should().BeTrue();
        hosp.Procedures.Should().HaveCount(1);
    }

    [Fact]
    public void Admit_WithEmptyIdentifiers_ReturnsFailure()
    {
        var result = Hospitalization.Admit(Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), Guid.NewGuid(), "Reason", DateTimeOffset.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Hospitalization.InvalidIdentifiers");
    }

    [Fact]
    public void Admit_WithBlankReason_ReturnsFailure()
    {
        var result = Hospitalization.Admit(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "  ", DateTimeOffset.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Hospitalization.InvalidReason");
    }

    [Fact]
    public void Discharge_WhenAlreadyDischarged_ReturnsFailure()
    {
        var hosp = CreateAdmitted();
        hosp.Discharge(DateTimeOffset.UtcNow);

        var result = hosp.Discharge(DateTimeOffset.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Hospitalization.AlreadyDischarged");
    }

    [Fact]
    public void TransferBed_WithSameBed_ReturnsFailure()
    {
        var hosp = CreateAdmitted();

        var result = hosp.TransferBed(hosp.BedId);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Hospitalization.InvalidBed");
    }

    [Fact]
    public void TransferBed_WhenDischarged_ReturnsFailure()
    {
        var hosp = CreateAdmitted();
        hosp.Discharge(DateTimeOffset.UtcNow);

        hosp.TransferBed(Guid.NewGuid()).IsFailure.Should().BeTrue();
    }

    [Fact]
    public void SkipMedication_WhenPending_SetsSkipped()
    {
        var hosp = CreateAdmitted();
        hosp.AddMedicationOrder(
            Guid.NewGuid(),
            "Dipyrone",
            "500mg",
            "IV",
            [new TimeOnly(8, 0)],
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow));

        var adminId = hosp.Administrations.First().Id;
        var result = hosp.SkipMedication(adminId, Guid.NewGuid(), "Nauseated", DateTimeOffset.UtcNow);

        result.IsSuccess.Should().BeTrue();
        hosp.Administrations.First(a => a.Id == adminId).Status.Should().Be(MedicationAdministrationStatus.Skipped);
    }

    [Fact]
    public void SkipMedication_WhenNotFound_ReturnsFailure()
    {
        var hosp = CreateAdmitted();

        var result = hosp.SkipMedication(Guid.NewGuid(), Guid.NewGuid(), null, DateTimeOffset.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("MedicationAdministration.NotFound");
    }

    [Fact]
    public void AdministerMedication_WhenNotFound_ReturnsFailure()
    {
        var hosp = CreateAdmitted();

        var result = hosp.AdministerMedication(Guid.NewGuid(), Guid.NewGuid(), null, DateTimeOffset.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("MedicationAdministration.NotFound");
    }

    [Fact]
    public void AddProgressNote_WhenDischarged_ReturnsFailure()
    {
        var hosp = CreateAdmitted();
        hosp.Discharge(DateTimeOffset.UtcNow);

        hosp.AddProgressNote(Guid.NewGuid(), Guid.NewGuid(), "Note", DateTimeOffset.UtcNow).IsFailure.Should().BeTrue();
    }

    [Fact]
    public void AddProgressNote_WithEmptyText_ReturnsFailure()
    {
        var hosp = CreateAdmitted();

        hosp.AddProgressNote(Guid.NewGuid(), Guid.NewGuid(), "  ", DateTimeOffset.UtcNow).IsFailure.Should().BeTrue();
    }

    [Fact]
    public void RestoreFromSync_AndApplySnapshot_RehydratesCollections()
    {
        var id = Guid.NewGuid();
        var petId = Guid.NewGuid();
        var vetId = Guid.NewGuid();
        var bedId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var noteId = Guid.NewGuid();
        var procId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        var restored = Hospitalization.RestoreFromSync(
            id,
            petId,
            vetId,
            bedId,
            "Dehydration",
            now.AddDays(-1),
            null,
            HospitalizationStatus.Admitted,
            now,
            [(orderId, "Dipyrone", "500mg", "IV", "08:00", DateOnly.FromDateTime(now.Date), DateOnly.FromDateTime(now.Date), HospitalMedicationOrderStatus.Active, now)],
            [(adminId, orderId, now, MedicationAdministrationStatus.Pending, null, null, "", now)],
            [(noteId, vetId, "Stable", now, now)],
            [(procId, "Fluids", vetId, now, "IV", now)]);

        restored.PetId.Should().Be(petId);
        restored.MedicationOrders.Should().ContainSingle(o => o.Id == orderId);
        restored.Administrations.Should().ContainSingle(a => a.Id == adminId);
        restored.ProgressNotes.Should().ContainSingle(n => n.Id == noteId);
        restored.Procedures.Should().ContainSingle(p => p.Id == procId);

        var newBed = Guid.NewGuid();
        restored.ApplySyncSnapshot(
            petId,
            vetId,
            newBed,
            "Updated reason",
            now.AddDays(-1),
            now,
            HospitalizationStatus.Discharged,
            now,
            Array.Empty<(Guid, string, string, string, string, DateOnly, DateOnly, HospitalMedicationOrderStatus, DateTimeOffset)>(),
            Array.Empty<(Guid, Guid, DateTimeOffset, MedicationAdministrationStatus, Guid?, DateTimeOffset?, string, DateTimeOffset)>(),
            Array.Empty<(Guid, Guid, string, DateTimeOffset, DateTimeOffset)>(),
            Array.Empty<(Guid, string, Guid, DateTimeOffset, string, DateTimeOffset)>());

        restored.BedId.Should().Be(newBed);
        restored.Status.Should().Be(HospitalizationStatus.Discharged);
        restored.MedicationOrders.Should().BeEmpty();
    }
}
