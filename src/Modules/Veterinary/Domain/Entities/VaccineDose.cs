using Core.Domain;
using Veterinary.Domain;

namespace Veterinary.Domain.Entities;

/// <summary>Immutable record of a vaccine application for a pet.</summary>
public class VaccineDose : AggregateRoot
{
    /// <summary>Patient pet.</summary>
    public Guid PetId { get; private set; }

    /// <summary>Applied vaccine name (from protocol label or free text).</summary>
    public string Name { get; private set; }

    /// <summary>Manufacturer batch number.</summary>
    public string BatchNumber { get; private set; }

    /// <summary>When the dose was applied (UTC).</summary>
    public DateTimeOffset AppliedAt { get; private set; }

    /// <summary>Scheduled next booster (UTC), when known.</summary>
    public DateTimeOffset? NextDueDate { get; private set; }

    /// <summary>Optional link to catalog protocol.</summary>
    public Guid? ProtocolId { get; private set; }

    /// <summary>Optional link to protocol dose step.</summary>
    public Guid? ProtocolDoseId { get; private set; }

    private VaccineDose()
    {
        Name = string.Empty;
        BatchNumber = string.Empty;
    }

    private VaccineDose(
        Guid id,
        Guid petId,
        string name,
        string batchNumber,
        DateTimeOffset appliedAt,
        DateTimeOffset? nextDueDate,
        Guid? protocolId,
        Guid? protocolDoseId)
    {
        Id = id;
        PetId = petId;
        Name = name;
        BatchNumber = batchNumber;
        AppliedAt = appliedAt;
        NextDueDate = nextDueDate;
        ProtocolId = protocolId;
        ProtocolDoseId = protocolDoseId;
    }

    /// <summary>Creates a new applied dose with domain validation.</summary>
    public static Result<VaccineDose> Create(
        Guid id,
        Guid petId,
        string name,
        string batchNumber,
        DateTimeOffset appliedAt,
        DateTimeOffset? nextDueDate,
        Guid? protocolId = null,
        Guid? protocolDoseId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<VaccineDose>(ErrorCodes.VaccineDose.InvalidName);
        }

        if (appliedAt > DateTimeOffset.UtcNow)
        {
            return Result.Failure<VaccineDose>(ErrorCodes.VaccineDose.FutureApplicationDate);
        }

        return Result<VaccineDose>.Success(new VaccineDose(
            id,
            petId,
            name.Trim(),
            batchNumber?.Trim() ?? string.Empty,
            appliedAt,
            nextDueDate,
            protocolId,
            protocolDoseId));
    }

    /// <summary>Rehydrates from sync pull without business validation.</summary>
    public static VaccineDose RestoreFromSync(
        Guid id,
        Guid petId,
        string name,
        string batchNumber,
        DateTimeOffset appliedAt,
        DateTimeOffset? nextDueDate,
        Guid? protocolId,
        Guid? protocolDoseId,
        DateTimeOffset updatedAt)
    {
        return new VaccineDose(id, petId, name, batchNumber, appliedAt, nextDueDate, protocolId, protocolDoseId)
        {
            UpdatedAt = updatedAt
        };
    }

    /// <summary>Applies remote sync snapshot (LWW).</summary>
    public void ApplySyncSnapshot(
        string name,
        string batchNumber,
        DateTimeOffset appliedAt,
        DateTimeOffset? nextDueDate,
        Guid? protocolId,
        Guid? protocolDoseId,
        DateTimeOffset updatedAt)
    {
        Name = name;
        BatchNumber = batchNumber;
        AppliedAt = appliedAt;
        NextDueDate = nextDueDate;
        ProtocolId = protocolId;
        ProtocolDoseId = protocolDoseId;
        UpdatedAt = updatedAt;
    }
}
