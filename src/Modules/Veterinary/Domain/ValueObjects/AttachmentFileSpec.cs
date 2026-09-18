using Core.Domain;
using Veterinary.Domain.Enums;

namespace Veterinary.Domain.ValueObjects;

/// <summary>
/// Validates upload constraints (MIME and size) before bytes are stored in blob storage.
/// </summary>
public sealed class AttachmentFileSpec
{
    private const long MaxPhotoBytes = 10 * 1024 * 1024;
    private const long MaxVideoBytes = 100 * 1024 * 1024;
    private const long MaxPdfBytes = 20 * 1024 * 1024;

    /// <summary>Detected attachment kind from content type.</summary>
    public ClinicalAttachmentKind Kind { get; }

    /// <summary>Original file name for download headers.</summary>
    public string FileName { get; }

    /// <summary>Declared content type.</summary>
    public string ContentType { get; }

    /// <summary>Payload size in bytes.</summary>
    public long SizeBytes { get; }

    private AttachmentFileSpec(ClinicalAttachmentKind kind, string fileName, string contentType, long sizeBytes)
    {
        Kind = kind;
        FileName = fileName;
        ContentType = contentType;
        SizeBytes = sizeBytes;
    }

    /// <summary>Validates MIME and size limits for clinical uploads.</summary>
    public static Result<AttachmentFileSpec> Create(string fileName, string contentType, long sizeBytes)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return Result.Failure<AttachmentFileSpec>(ErrorCodes.ClinicalAttachment.InvalidFileName);
        }

        if (sizeBytes <= 0)
        {
            return Result.Failure<AttachmentFileSpec>(ErrorCodes.ClinicalAttachment.InvalidSize);
        }

        var normalizedType = contentType.Trim().ToLowerInvariant();
        var kindResult = ResolveKind(normalizedType);
        if (kindResult.IsFailure)
        {
            return Result.Failure<AttachmentFileSpec>(kindResult.Error);
        }

        var max = kindResult.Value switch
        {
            ClinicalAttachmentKind.Photo => MaxPhotoBytes,
            ClinicalAttachmentKind.Video => MaxVideoBytes,
            ClinicalAttachmentKind.Pdf => MaxPdfBytes,
            _ => MaxPdfBytes
        };

        if (sizeBytes > max)
        {
            return Result.Failure<AttachmentFileSpec>(ErrorCodes.ClinicalAttachment.FileTooLarge);
        }

        return Result.Success(new AttachmentFileSpec(kindResult.Value, fileName.Trim(), normalizedType, sizeBytes));
    }

    private static Result<ClinicalAttachmentKind> ResolveKind(string contentType) =>
        contentType switch
        {
            "image/jpeg" or "image/png" or "image/webp" => Result.Success(ClinicalAttachmentKind.Photo),
            "video/mp4" or "video/webm" => Result.Success(ClinicalAttachmentKind.Video),
            "application/pdf" => Result.Success(ClinicalAttachmentKind.Pdf),
            _ => Result.Failure<ClinicalAttachmentKind>(ErrorCodes.ClinicalAttachment.UnsupportedContentType)
        };
}
