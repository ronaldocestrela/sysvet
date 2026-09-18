using Core.Domain;
using Veterinary.Domain;
using Veterinary.Domain.ValueObjects;

namespace Veterinary.Domain.Entities;

/// <summary>Lifecycle state of a consultation medical record.</summary>
public enum MedicalRecordStatus
{
    Draft = 1,
    Finalized = 2
}

/// <summary>
/// Aggregate root for a veterinary consultation record linked 1:1 to an appointment.
/// </summary>
public class MedicalRecord : AggregateRoot
{
    private readonly List<EvolutionNote> _evolutionNotes = new();

    /// <summary>Linked appointment (unique per record).</summary>
    public Guid AppointmentId { get; private set; }

    /// <summary>Attending veterinarian.</summary>
    public Guid VeterinarianId { get; private set; }

    /// <summary>Pet owner for CRM context.</summary>
    public Guid TutorId { get; private set; }

    /// <summary>Patient pet.</summary>
    public Guid PetId { get; private set; }

    /// <summary>Patient history and chief complaint narrative.</summary>
    public string Anamnesis { get; private set; } = string.Empty;

    /// <summary>Latest vital signs snapshot for this visit.</summary>
    public VitalSigns? VitalSigns { get; private set; }

    /// <summary>Clinical diagnosis for the visit.</summary>
    public string Diagnosis { get; private set; } = string.Empty;

    /// <summary>Conduct / treatment plan (formal prescriptions are phase 4.3).</summary>
    public string Prescription { get; private set; } = string.Empty;

    /// <summary>Draft allows edits; finalized is immutable.</summary>
    public MedicalRecordStatus Status { get; private set; }

    /// <summary>Append-only evolution entries for this consultation.</summary>
    public IReadOnlyCollection<EvolutionNote> EvolutionNotes => _evolutionNotes.AsReadOnly();

    private MedicalRecord() { }

    private MedicalRecord(Guid id, Guid appointmentId, Guid veterinarianId, Guid tutorId, Guid petId)
        : base(id)
    {
        AppointmentId = appointmentId;
        VeterinarianId = veterinarianId;
        TutorId = tutorId;
        PetId = petId;
        Status = MedicalRecordStatus.Draft;
    }

    /// <summary>Creates a new draft medical record for an eligible appointment.</summary>
    public static Result<MedicalRecord> Create(Guid id, Guid appointmentId, Guid veterinarianId, Guid tutorId, Guid petId)
    {
        if (appointmentId == Guid.Empty || veterinarianId == Guid.Empty || tutorId == Guid.Empty || petId == Guid.Empty)
        {
            return Result.Failure<MedicalRecord>(ErrorCodes.MedicalRecord.InvalidIdentifiers);
        }

        var record = new MedicalRecord(id, appointmentId, veterinarianId, tutorId, petId);
        record.Touch();
        return Result.Success(record);
    }

    /// <summary>Rehydrates a record from sync pull without business validation.</summary>
    public static MedicalRecord RestoreFromSync(
        Guid id,
        Guid appointmentId,
        Guid veterinarianId,
        Guid tutorId,
        Guid petId,
        string anamnesis,
        string diagnosis,
        string prescription,
        MedicalRecordStatus status,
        DateTimeOffset updatedAt,
        VitalSigns? vitalSigns,
        IEnumerable<(Guid NoteId, Guid AuthorId, string Text, DateTimeOffset RecordedAt)> evolutionNotes)
    {
        var record = new MedicalRecord(id, appointmentId, veterinarianId, tutorId, petId)
        {
            Anamnesis = anamnesis,
            Diagnosis = diagnosis,
            Prescription = prescription,
            Status = status,
            VitalSigns = vitalSigns,
            UpdatedAt = updatedAt
        };

        foreach (var note in evolutionNotes)
        {
            var created = EvolutionNote.Create(note.NoteId, id, note.AuthorId, note.Text, note.RecordedAt);
            if (created.IsSuccess)
            {
                record._evolutionNotes.Add(created.Value);
            }
        }

        return record;
    }

    /// <summary>
    /// Applies remote sync fields when remote wins LWW; never downgrades a locally finalized record to draft.
    /// </summary>
    public void ApplySyncSnapshot(
        string anamnesis,
        string diagnosis,
        string prescription,
        MedicalRecordStatus status,
        VitalSigns? vitalSigns,
        DateTimeOffset updatedAt,
        IEnumerable<(Guid NoteId, Guid AuthorId, string Text, DateTimeOffset RecordedAt)> evolutionNotes)
    {
        if (Status == MedicalRecordStatus.Finalized && status == MedicalRecordStatus.Draft)
        {
            status = MedicalRecordStatus.Finalized;
        }

        Anamnesis = anamnesis;
        Diagnosis = diagnosis;
        Prescription = prescription;
        Status = status;
        VitalSigns = vitalSigns;
        UpdatedAt = updatedAt;

        _evolutionNotes.Clear();
        foreach (var note in evolutionNotes)
        {
            var created = EvolutionNote.Create(note.NoteId, Id, note.AuthorId, note.Text, note.RecordedAt);
            if (created.IsSuccess)
            {
                _evolutionNotes.Add(created.Value);
            }
        }
    }

    /// <summary>Replaces anamnesis while the record is editable.</summary>
    public Result SetAnamnesis(string anamnesis)
    {
        if (!EnsureDraft())
        {
            return Result.Failure(ErrorCodes.MedicalRecord.Finalized);
        }

        if (anamnesis.Length > 4000)
        {
            return Result.Failure(ErrorCodes.MedicalRecord.InvalidAnamnesis);
        }

        Anamnesis = anamnesis.Trim();
        Touch();
        return Result.Success();
    }

    /// <summary>Replaces vital signs snapshot while the record is editable.</summary>
    public Result RecordVitalSigns(VitalSigns vitalSigns)
    {
        if (!EnsureDraft())
        {
            return Result.Failure(ErrorCodes.MedicalRecord.Finalized);
        }

        VitalSigns = vitalSigns;
        Touch();
        return Result.Success();
    }

    /// <summary>Appends a clinical evolution note.</summary>
    public Result<Guid> AddEvolutionNote(Guid noteId, Guid authorId, string text, DateTimeOffset recordedAt)
    {
        if (!EnsureDraft())
        {
            return Result.Failure<Guid>(ErrorCodes.MedicalRecord.Finalized);
        }

        var noteResult = EvolutionNote.Create(noteId, Id, authorId, text, recordedAt);
        if (noteResult.IsFailure)
        {
            return Result.Failure<Guid>(noteResult.Error);
        }

        _evolutionNotes.Add(noteResult.Value);
        Touch();
        return Result.Success(noteId);
    }

    /// <summary>Replaces diagnosis text (legacy append kept for backward compatibility).</summary>
    public Result<bool> SetDiagnosis(string diagnosis)
    {
        if (!EnsureDraft())
        {
            return Result.Failure<bool>(ErrorCodes.MedicalRecord.Finalized);
        }

        if (diagnosis.Length > 2000)
        {
            return Result.Failure<bool>(ErrorCodes.MedicalRecord.InvalidDiagnosis);
        }

        Diagnosis = diagnosis.Trim();
        Touch();
        return Result.Success(true);
    }

    /// <summary>Replaces conduct / plan text.</summary>
    public Result<bool> SetConduct(string conduct)
    {
        if (!EnsureDraft())
        {
            return Result.Failure<bool>(ErrorCodes.MedicalRecord.Finalized);
        }

        if (conduct.Length > 2000)
        {
            return Result.Failure<bool>(ErrorCodes.MedicalRecord.InvalidConduct);
        }

        Prescription = conduct.Trim();
        Touch();
        return Result.Success(true);
    }

    /// <summary>Appends diagnosis lines (deprecated for UI; prefer <see cref="SetDiagnosis"/>).</summary>
    public Result<bool> AppendDiagnosis(string diagnosis)
    {
        if (Status == MedicalRecordStatus.Finalized)
        {
            return Result.Failure<bool>(ErrorCodes.MedicalRecord.Finalized);
        }

        Diagnosis += string.IsNullOrEmpty(Diagnosis) ? diagnosis : "\n" + diagnosis;
        Touch();
        return Result.Success(true);
    }

    /// <summary>Appends conduct lines (deprecated for UI; prefer <see cref="SetConduct"/>).</summary>
    public Result<bool> AppendPrescription(string prescription)
    {
        if (Status == MedicalRecordStatus.Finalized)
        {
            return Result.Failure<bool>(ErrorCodes.MedicalRecord.Finalized);
        }

        Prescription += string.IsNullOrEmpty(Prescription) ? prescription : "\n" + prescription;
        Touch();
        return Result.Success(true);
    }

    /// <summary>Locks the record against further clinical edits.</summary>
    public Result<bool> FinalizeRecord()
    {
        if (Status == MedicalRecordStatus.Finalized)
        {
            return Result.Failure<bool>(ErrorCodes.MedicalRecord.AlreadyFinalized);
        }

        Status = MedicalRecordStatus.Finalized;
        Touch();
        return Result.Success(true);
    }

    private bool EnsureDraft() => Status != MedicalRecordStatus.Finalized;

    private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
}
