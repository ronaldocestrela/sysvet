using FluentAssertions;
using Veterinary.Domain.Enums;
using Veterinary.Domain.ValueObjects;

namespace Veterinary.Domain.Tests;

public class AttachmentFileSpecTests
{
    [Fact]
    public void Create_WithPdf_ReturnsPdfKind()
    {
        var result = AttachmentFileSpec.Create("report.pdf", "application/pdf", 1024);

        result.IsSuccess.Should().BeTrue();
        result.Value.Kind.Should().Be(ClinicalAttachmentKind.Pdf);
    }

    [Fact]
    public void Create_WithUnsupportedMime_ReturnsFailure()
    {
        var result = AttachmentFileSpec.Create("file.exe", "application/octet-stream", 100);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ClinicalAttachment.UnsupportedContentType");
    }

    [Fact]
    public void Create_WhenPhotoTooLarge_ReturnsFailure()
    {
        var result = AttachmentFileSpec.Create("photo.jpg", "image/jpeg", 11 * 1024 * 1024);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ClinicalAttachment.FileTooLarge");
    }
}
