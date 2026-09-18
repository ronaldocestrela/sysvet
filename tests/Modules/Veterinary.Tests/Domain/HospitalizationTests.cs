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
}
