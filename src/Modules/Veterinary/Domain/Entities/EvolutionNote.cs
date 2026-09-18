using Core.Domain;
using Veterinary.Domain;

namespace Veterinary.Domain.Entities;

/// <summary>
/// Append-only clinical evolution entry belonging to a medical record aggregate.
/// </summary>
public sealed class EvolutionNote : Entity
{
    /// <summary>Parent medical record identifier.</summary>
    public Guid MedicalRecordId { get; private set; }

    /// <summary>Staff member who recorded the note.</summary>
    public Guid AuthorId { get; private set; }

    /// <summary>Clinical evolution text.</summary>
    public string Text { get; private set; } = string.Empty;

    /// <summary>When the note was recorded (UTC).</summary>
    public DateTimeOffset RecordedAt { get; private set; }

    private EvolutionNote() { }

    private EvolutionNote(Guid id, Guid medicalRecordId, Guid authorId, string text, DateTimeOffset recordedAt)
        : base(id)
    {
        MedicalRecordId = medicalRecordId;
        AuthorId = authorId;
        Text = text;
        RecordedAt = recordedAt;
    }

    /// <summary>Creates a new evolution note for the aggregate.</summary>
    public static Result<EvolutionNote> Create(Guid id, Guid medicalRecordId, Guid authorId, string text, DateTimeOffset recordedAt)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Result.Failure<EvolutionNote>(ErrorCodes.MedicalRecord.EmptyEvolution);
        }

        if (text.Length > 4000)
        {
            return Result.Failure<EvolutionNote>(ErrorCodes.MedicalRecord.InvalidEvolution);
        }

        if (authorId == Guid.Empty)
        {
            return Result.Failure<EvolutionNote>(ErrorCodes.MedicalRecord.EmptyEvolution);
        }

        return Result.Success(new EvolutionNote(id, medicalRecordId, authorId, text.Trim(), recordedAt));
    }
}
