using Core.Domain;
using Veterinary.Domain;
using Veterinary.Domain.Enums;
using Veterinary.Domain.ValueObjects;

namespace Veterinary.Domain.Entities;

/// <summary>
/// Metadata for a file stored in blob storage; binary content is never held on the aggregate.
/// </summary>
public sealed class ClinicalAttachment : AggregateRoot
{
    /// <summary>Consultation appointment (required link for acceptance criteria).</summary>
    public Guid AppointmentId { get; private set; }

    /// <summary>Optional medical record link.</summary>
    public Guid? MedicalRecordId { get; private set; }

    /// <summary>Optional related exam.</summary>
    public Guid? ClinicalExamId { get; private set; }

    /// <summary>Original upload file name.</summary>
    public string FileName { get; private set; } = string.Empty;

    /// <summary>Stored content type.</summary>
    public string ContentType { get; private set; } = string.Empty;

    /// <summary>Payload size in bytes.</summary>
    public long SizeBytes { get; private set; }

    /// <summary>Media kind derived from content type.</summary>
    public ClinicalAttachmentKind Kind { get; private set; }

    /// <summary>Storage key (tenant-scoped path in blob provider).</summary>
    public string BlobKey { get; private set; } = string.Empty;

    /// <summary>Soft-delete tombstone for sync.</summary>
    public bool IsDeleted { get; private set; }

    private ClinicalAttachment() { }

    private ClinicalAttachment(
        Guid id,
        Guid appointmentId,
        Guid? medicalRecordId,
        Guid? clinicalExamId,
        AttachmentFileSpec spec,
        string blobKey)
        : base(id)
    {
        AppointmentId = appointmentId;
        MedicalRecordId = medicalRecordId;
        ClinicalExamId = clinicalExamId;
        FileName = spec.FileName;
        ContentType = spec.ContentType;
        SizeBytes = spec.SizeBytes;
        Kind = spec.Kind;
        BlobKey = blobKey;
    }

    /// <summary>Creates metadata after blob upload succeeded.</summary>
    public static Result<ClinicalAttachment> Create(
        Guid id,
        Guid appointmentId,
        Guid? medicalRecordId,
        Guid? clinicalExamId,
        AttachmentFileSpec spec,
        string blobKey)
    {
        if (appointmentId == Guid.Empty)
        {
            return Result.Failure<ClinicalAttachment>(ErrorCodes.ClinicalAttachment.InvalidIdentifiers);
        }

        if (string.IsNullOrWhiteSpace(blobKey))
        {
            return Result.Failure<ClinicalAttachment>(ErrorCodes.ClinicalAttachment.InvalidBlobKey);
        }

        var attachment = new ClinicalAttachment(id, appointmentId, medicalRecordId, clinicalExamId, spec, blobKey);
        attachment.Touch();
        return Result.Success(attachment);
    }

    /// <summary>Rehydrates from sync (no file spec validation).</summary>
    public static ClinicalAttachment RestoreFromSync(
        Guid id,
        Guid appointmentId,
        Guid? medicalRecordId,
        Guid? clinicalExamId,
        string fileName,
        string contentType,
        long sizeBytes,
        ClinicalAttachmentKind kind,
        string blobKey,
        bool isDeleted,
        DateTimeOffset updatedAt)
    {
        var attachment = new ClinicalAttachment
        {
            Id = id,
            AppointmentId = appointmentId,
            MedicalRecordId = medicalRecordId,
            ClinicalExamId = clinicalExamId,
            FileName = fileName,
            ContentType = contentType,
            SizeBytes = sizeBytes,
            Kind = kind,
            BlobKey = blobKey,
            IsDeleted = isDeleted,
            UpdatedAt = updatedAt
        };
        return attachment;
    }

    /// <summary>Applies remote sync snapshot.</summary>
    public void ApplySyncSnapshot(
        Guid appointmentId,
        Guid? medicalRecordId,
        Guid? clinicalExamId,
        string fileName,
        string contentType,
        long sizeBytes,
        ClinicalAttachmentKind kind,
        string blobKey,
        bool isDeleted,
        DateTimeOffset updatedAt)
    {
        AppointmentId = appointmentId;
        MedicalRecordId = medicalRecordId;
        ClinicalExamId = clinicalExamId;
        FileName = fileName;
        ContentType = contentType;
        SizeBytes = sizeBytes;
        Kind = kind;
        BlobKey = blobKey;
        IsDeleted = isDeleted;
        UpdatedAt = updatedAt;
    }

    /// <summary>Soft-deletes the attachment metadata (blob may be purged asynchronously later).</summary>
    public Result SoftDelete()
    {
        if (IsDeleted)
        {
            return Result.Failure(ErrorCodes.ClinicalAttachment.AlreadyDeleted);
        }

        IsDeleted = true;
        Touch();
        return Result.Success();
    }

    private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
}
