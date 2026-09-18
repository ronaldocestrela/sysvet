using Core.Domain;
using Veterinary.Domain;
using Veterinary.Domain.Enums;

namespace Veterinary.Domain.Entities;

/// <summary>Materialized slot for administering an inpatient medication order.</summary>
public sealed class MedicationAdministration : Entity
{
    /// <summary>Parent hospitalization.</summary>
    public Guid HospitalizationId { get; private set; }

    /// <summary>Source medication order.</summary>
    public Guid MedicationOrderId { get; private set; }

    /// <summary>When the dose should be given (UTC).</summary>
    public DateTimeOffset ScheduledAt { get; private set; }

    /// <summary>Slot workflow status.</summary>
    public MedicationAdministrationStatus Status { get; private set; }

    /// <summary>Staff who administered or skipped.</summary>
    public Guid? ActorId { get; private set; }

    /// <summary>When the action was recorded (UTC).</summary>
    public DateTimeOffset? ActedAt { get; private set; }

    /// <summary>Optional notes for administer/skip.</summary>
    public string Notes { get; private set; } = string.Empty;

    private MedicationAdministration() { }

    private MedicationAdministration(Guid id, Guid hospitalizationId, Guid medicationOrderId, DateTimeOffset scheduledAt)
        : base(id)
    {
        HospitalizationId = hospitalizationId;
        MedicationOrderId = medicationOrderId;
        ScheduledAt = scheduledAt;
        Status = MedicationAdministrationStatus.Pending;
    }

    /// <summary>Creates a pending administration slot.</summary>
    internal static Result<MedicationAdministration> CreatePending(
        Guid id,
        Guid hospitalizationId,
        Guid medicationOrderId,
        DateTimeOffset scheduledAt)
    {
        if (hospitalizationId == Guid.Empty || medicationOrderId == Guid.Empty)
        {
            return Result.Failure<MedicationAdministration>(ErrorCodes.Hospitalization.InvalidIdentifiers);
        }

        return Result.Success(new MedicationAdministration(id, hospitalizationId, medicationOrderId, scheduledAt));
    }

    /// <summary>Rehydrates from sync.</summary>
    internal static MedicationAdministration RestoreFromSync(
        Guid id,
        Guid hospitalizationId,
        Guid medicationOrderId,
        DateTimeOffset scheduledAt,
        MedicationAdministrationStatus status,
        Guid? actorId,
        DateTimeOffset? actedAt,
        string notes,
        DateTimeOffset updatedAt)
    {
        return new MedicationAdministration(id, hospitalizationId, medicationOrderId, scheduledAt)
        {
            Status = status,
            ActorId = actorId,
            ActedAt = actedAt,
            Notes = notes ?? string.Empty,
            UpdatedAt = updatedAt
        };
    }

    /// <summary>Records successful administration.</summary>
    internal Result Administer(Guid actorId, string? notes, DateTimeOffset actedAt)
    {
        if (Status != MedicationAdministrationStatus.Pending)
        {
            return Result.Failure(ErrorCodes.MedicationAdministration.InvalidTransition);
        }

        if (actorId == Guid.Empty)
        {
            return Result.Failure(ErrorCodes.MedicationAdministration.InvalidActor);
        }

        Status = MedicationAdministrationStatus.Administered;
        ActorId = actorId;
        ActedAt = actedAt;
        Notes = notes?.Trim() ?? string.Empty;
        UpdatedAt = actedAt;
        return Result.Success();
    }

    /// <summary>Records intentional skip.</summary>
    internal Result Skip(Guid actorId, string? notes, DateTimeOffset actedAt)
    {
        if (Status != MedicationAdministrationStatus.Pending)
        {
            return Result.Failure(ErrorCodes.MedicationAdministration.InvalidTransition);
        }

        if (actorId == Guid.Empty)
        {
            return Result.Failure(ErrorCodes.MedicationAdministration.InvalidActor);
        }

        Status = MedicationAdministrationStatus.Skipped;
        ActorId = actorId;
        ActedAt = actedAt;
        Notes = notes?.Trim() ?? string.Empty;
        UpdatedAt = actedAt;
        return Result.Success();
    }

    /// <summary>Cancels a pending slot (e.g. on discharge).</summary>
    internal void CancelPending(DateTimeOffset cancelledAt)
    {
        if (Status != MedicationAdministrationStatus.Pending)
        {
            return;
        }

        Status = MedicationAdministrationStatus.Cancelled;
        ActedAt = cancelledAt;
        UpdatedAt = cancelledAt;
    }
}
