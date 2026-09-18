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
}
