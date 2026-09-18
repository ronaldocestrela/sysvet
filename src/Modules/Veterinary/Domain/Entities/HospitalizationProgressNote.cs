using Core.Domain;
using Veterinary.Domain;

namespace Veterinary.Domain.Entities;

/// <summary>Append-only daily evolution note for an inpatient stay.</summary>
public sealed class HospitalizationProgressNote : Entity
{
    /// <summary>Parent hospitalization.</summary>
    public Guid HospitalizationId { get; private set; }

    /// <summary>Author staff user.</summary>
    public Guid AuthorId { get; private set; }

    /// <summary>Clinical evolution text.</summary>
    public string Text { get; private set; } = string.Empty;

    /// <summary>When recorded (UTC).</summary>
    public DateTimeOffset RecordedAt { get; private set; }

    private HospitalizationProgressNote() { }

    private HospitalizationProgressNote(Guid id, Guid hospitalizationId, Guid authorId, string text, DateTimeOffset recordedAt)
        : base(id)
    {
        HospitalizationId = hospitalizationId;
        AuthorId = authorId;
        Text = text;
        RecordedAt = recordedAt;
    }

    /// <summary>Creates a progress note.</summary>
    internal static Result<HospitalizationProgressNote> Create(
        Guid id,
        Guid hospitalizationId,
        Guid authorId,
        string text,
        DateTimeOffset recordedAt)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Result.Failure<HospitalizationProgressNote>(ErrorCodes.HospitalizationProgressNote.EmptyText);
        }

        if (text.Length > 4000)
        {
            return Result.Failure<HospitalizationProgressNote>(ErrorCodes.HospitalizationProgressNote.InvalidText);
        }

        if (authorId == Guid.Empty)
        {
            return Result.Failure<HospitalizationProgressNote>(ErrorCodes.HospitalizationProgressNote.EmptyText);
        }

        return Result.Success(new HospitalizationProgressNote(id, hospitalizationId, authorId, text.Trim(), recordedAt));
    }

    /// <summary>Rehydrates from sync.</summary>
    internal static HospitalizationProgressNote RestoreFromSync(
        Guid id,
        Guid hospitalizationId,
        Guid authorId,
        string text,
        DateTimeOffset recordedAt,
        DateTimeOffset updatedAt)
    {
        return new HospitalizationProgressNote(id, hospitalizationId, authorId, text, recordedAt) { UpdatedAt = updatedAt };
    }
}
