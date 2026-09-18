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
}
