using FluentAssertions;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Enums;

namespace Veterinary.Domain.Tests;

public class ClinicalExamTests
{
    [Fact]
    public void Request_WithValidData_ReturnsSuccess()
    {
        var result = ClinicalExam.Request(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Blood panel", ClinicalExamCategory.Laboratory);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(ClinicalExamStatus.Requested);
    }

    [Fact]
    public void Complete_FromRequested_SetsResultAndStatus()
    {
        var exam = ClinicalExam.Request(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "X-Ray", ClinicalExamCategory.Imaging).Value;

        var result = exam.Complete("No fractures");

        result.IsSuccess.Should().BeTrue();
        exam.Status.Should().Be(ClinicalExamStatus.Completed);
        exam.ResultSummary.Should().Be("No fractures");
    }

    [Fact]
    public void Complete_WhenCancelled_ReturnsFailure()
    {
        var exam = ClinicalExam.Request(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "X-Ray", ClinicalExamCategory.Imaging).Value;
        exam.Cancel();

        var result = exam.Complete("Late result");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ClinicalExam.InvalidTransition");
    }
}
