using Core.Domain;
using Veterinary.Domain;
using Veterinary.Domain.Enums;

namespace Veterinary.Domain.Entities;

/// <summary>Clinical exam order or result linked to a consultation.</summary>
public sealed class ClinicalExam : AggregateRoot
{
    /// <summary>Consultation appointment.</summary>
    public Guid AppointmentId { get; private set; }

    /// <summary>Patient pet.</summary>
    public Guid PetId { get; private set; }

    /// <summary>Exam display name.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Exam category.</summary>
    public ClinicalExamCategory Category { get; private set; }

    /// <summary>Workflow status.</summary>
    public ClinicalExamStatus Status { get; private set; }

    /// <summary>Result narrative when completed.</summary>
    public string ResultSummary { get; private set; } = string.Empty;

    private ClinicalExam() { }

    private ClinicalExam(Guid id, Guid appointmentId, Guid petId, string name, ClinicalExamCategory category)
        : base(id)
    {
        AppointmentId = appointmentId;
        PetId = petId;
        Name = name;
        Category = category;
        Status = ClinicalExamStatus.Requested;
    }

    /// <summary>Requests a new exam for a visit.</summary>
    public static Result<ClinicalExam> Request(Guid id, Guid appointmentId, Guid petId, string name, ClinicalExamCategory category)
    {
        if (appointmentId == Guid.Empty || petId == Guid.Empty)
        {
            return Result.Failure<ClinicalExam>(ErrorCodes.ClinicalExam.InvalidIdentifiers);
        }

        if (string.IsNullOrWhiteSpace(name) || name.Length > 200)
        {
            return Result.Failure<ClinicalExam>(ErrorCodes.ClinicalExam.InvalidName);
        }

        var exam = new ClinicalExam(id, appointmentId, petId, name.Trim(), category);
        exam.Touch();
        return Result.Success(exam);
    }

    /// <summary>Rehydrates from sync.</summary>
    public static ClinicalExam RestoreFromSync(
        Guid id,
        Guid appointmentId,
        Guid petId,
        string name,
        ClinicalExamCategory category,
        ClinicalExamStatus status,
        string resultSummary,
        DateTimeOffset updatedAt)
    {
        var exam = new ClinicalExam(id, appointmentId, petId, name, category)
        {
            Status = status,
            ResultSummary = resultSummary,
            UpdatedAt = updatedAt
        };
        return exam;
    }

    /// <summary>Applies remote sync snapshot.</summary>
    public void ApplySyncSnapshot(
        Guid appointmentId,
        Guid petId,
        string name,
        ClinicalExamCategory category,
        ClinicalExamStatus status,
        string resultSummary,
        DateTimeOffset updatedAt)
    {
        AppointmentId = appointmentId;
        PetId = petId;
        Name = name;
        Category = category;
        Status = status;
        ResultSummary = resultSummary;
        UpdatedAt = updatedAt;
    }

    /// <summary>Records completion with optional result text.</summary>
    public Result Complete(string? resultSummary)
    {
        if (Status != ClinicalExamStatus.Requested)
        {
            return Result.Failure(ErrorCodes.ClinicalExam.InvalidTransition);
        }

        var summary = resultSummary?.Trim() ?? string.Empty;
        if (summary.Length > 4000)
        {
            return Result.Failure(ErrorCodes.ClinicalExam.InvalidResult);
        }

        ResultSummary = summary;
        Status = ClinicalExamStatus.Completed;
        Touch();
        return Result.Success();
    }

    /// <summary>Cancels a requested exam.</summary>
    public Result Cancel()
    {
        if (Status != ClinicalExamStatus.Requested)
        {
            return Result.Failure(ErrorCodes.ClinicalExam.InvalidTransition);
        }

        Status = ClinicalExamStatus.Cancelled;
        Touch();
        return Result.Success();
    }

    private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
}
