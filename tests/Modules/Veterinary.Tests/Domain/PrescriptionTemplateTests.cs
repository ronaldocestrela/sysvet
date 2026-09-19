using FluentAssertions;
using Veterinary.Domain.Entities;

namespace Veterinary.Domain.Tests;

public class PrescriptionTemplateTests
{
    [Fact]
    public void ReplaceItems_WithValidLines_UpdatesCollection()
    {
        var template = PrescriptionTemplate.Create(Guid.NewGuid(), "Post-op canine").Value;

        var result = template.ReplaceItems([
            (Guid.NewGuid(), "Meloxicam", "2mg/ml", "0.2mg/kg", "PO", "24/24h", "3 days", "With food", 0)
        ]);

        result.IsSuccess.Should().BeTrue();
        template.Items.Should().HaveCount(1);
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ReturnsFailure()
    {
        var template = PrescriptionTemplate.Create(Guid.NewGuid(), "Template A").Value;
        template.Deactivate();

        var result = template.Deactivate();

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PrescriptionTemplate.AlreadyInactive");
    }

    [Fact]
    public void Create_WithEmptyName_ReturnsFailure()
    {
        PrescriptionTemplate.Create(Guid.NewGuid(), "  ").IsFailure.Should().BeTrue();
    }

    [Fact]
    public void UpdateDetails_WithValidName_UpdatesSpecies()
    {
        var template = PrescriptionTemplate.Create(Guid.NewGuid(), "Template A").Value;

        var result = template.UpdateDetails("Template B", "Canine");

        result.IsSuccess.Should().BeTrue();
        template.Name.Should().Be("Template B");
        template.Species.Should().Be("Canine");
    }

    [Fact]
    public void RestoreFromSync_AndApplySnapshot_ReplacesItems()
    {
        var id = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var restored = PrescriptionTemplate.RestoreFromSync(
            id,
            "Sync template",
            "Feline",
            true,
            now,
            [(itemId, "Drug", "10mg", "1 tab", "PO", "24h", "5d", "", 0)]);

        restored.Items.Should().HaveCount(1);

        restored.ApplySyncSnapshot(
            "Renamed",
            "Canine",
            false,
            now,
            Array.Empty<(Guid, string, string, string, string, string, string, string, int)>());

        restored.Name.Should().Be("Renamed");
        restored.IsActive.Should().BeFalse();
        restored.Items.Should().BeEmpty();
    }
}
