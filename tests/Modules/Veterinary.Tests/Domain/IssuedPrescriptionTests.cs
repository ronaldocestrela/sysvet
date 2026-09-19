using FluentAssertions;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Enums;

namespace Veterinary.Domain.Tests;

public class IssuedPrescriptionTests
{
    [Fact]
    public void Issue_WithoutItems_ReturnsFailure()
    {
        var prescription = IssuedPrescription.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()).Value;

        var result = prescription.Issue();

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("IssuedPrescription.EmptyItems");
    }

    [Fact]
    public void Issue_WithItems_LocksPrescription()
    {
        var id = Guid.NewGuid();
        var prescription = IssuedPrescription.Create(id, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()).Value;
        prescription.ReplaceDraftItems([
            (Guid.NewGuid(), "Amoxicillin", "500mg", "1 tab", "PO", "12/12h", "7 days", "", 0)
        ]);

        var result = prescription.Issue();

        result.IsSuccess.Should().BeTrue();
        prescription.Status.Should().Be(IssuedPrescriptionStatus.Issued);
        prescription.ReplaceDraftItems([]).IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_WithEmptyIdentifiers_ReturnsFailure()
    {
        var result = IssuedPrescription.Create(Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("IssuedPrescription.InvalidIdentifiers");
    }

    [Fact]
    public void CopyItemsFromTemplate_WhenDraft_CopiesLinesAndTemplateId()
    {
        var template = PrescriptionTemplate.Create(Guid.NewGuid(), "Canine post-op").Value;
        template.ReplaceItems([
            (Guid.NewGuid(), "Meloxicam", "2mg/ml", "0.2mg/kg", "PO", "24/24h", "3 days", "With food", 0)
        ]);
        var prescription = IssuedPrescription.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()).Value;

        var result = prescription.CopyItemsFromTemplate(template);

        result.IsSuccess.Should().BeTrue();
        prescription.TemplateId.Should().Be(template.Id);
        prescription.Items.Should().ContainSingle(i => i.MedicationName == "Meloxicam");
    }

    [Fact]
    public void RestoreFromSync_AndApplySnapshot_DoesNotDowngradeIssued()
    {
        var id = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        var restored = IssuedPrescription.RestoreFromSync(
            id,
            appointmentId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            IssuedPrescriptionStatus.Issued,
            now,
            [(itemId, "Amox", "500mg", "1 tab", "PO", "12/12h", "7d", "", 0)]);

        restored.Status.Should().Be(IssuedPrescriptionStatus.Issued);
        restored.Items.Should().HaveCount(1);

        restored.ApplySyncSnapshot(
            appointmentId,
            restored.PetId,
            restored.VeterinarianId,
            null,
            IssuedPrescriptionStatus.Draft,
            now,
            [(itemId, "Amox", "500mg", "1 tab", "PO", "12/12h", "7d", "", 0)]);

        restored.Status.Should().Be(IssuedPrescriptionStatus.Issued);
    }
}
