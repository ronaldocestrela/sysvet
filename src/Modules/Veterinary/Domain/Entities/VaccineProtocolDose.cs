using Core.Domain;
using Veterinary.Domain;

namespace Veterinary.Domain.Entities;

/// <summary>Single step within a tenant vaccine protocol (species/age scoped).</summary>
public sealed class VaccineProtocolDose : Entity
{
    /// <summary>Parent protocol identifier.</summary>
    public Guid VaccineProtocolId { get; private set; }

    /// <summary>1-based order within the protocol.</summary>
    public int Sequence { get; private set; }

    /// <summary>Display label (e.g. first dose).</summary>
    public string Label { get; private set; } = string.Empty;

    /// <summary>Minimum pet age in days to apply this step.</summary>
    public int MinAgeInDays { get; private set; }

    /// <summary>Maximum pet age in days (optional upper bound).</summary>
    public int? MaxAgeInDays { get; private set; }

    /// <summary>Days after previous dose before this step (optional for first dose).</summary>
    public int? IntervalFromPreviousInDays { get; private set; }

    /// <summary>Days after this application until the next booster is due.</summary>
    public int? NextDoseIntervalInDays { get; private set; }

    private VaccineProtocolDose() { }

    private VaccineProtocolDose(
        Guid id,
        Guid protocolId,
        int sequence,
        string label,
        int minAgeInDays,
        int? maxAgeInDays,
        int? intervalFromPreviousInDays,
        int? nextDoseIntervalInDays)
        : base(id)
    {
        VaccineProtocolId = protocolId;
        Sequence = sequence;
        Label = label;
        MinAgeInDays = minAgeInDays;
        MaxAgeInDays = maxAgeInDays;
        IntervalFromPreviousInDays = intervalFromPreviousInDays;
        NextDoseIntervalInDays = nextDoseIntervalInDays;
    }

    /// <summary>Creates a validated protocol dose line.</summary>
    public static Result<VaccineProtocolDose> Create(
        Guid id,
        Guid protocolId,
        int sequence,
        string label,
        int minAgeInDays,
        int? maxAgeInDays,
        int? intervalFromPreviousInDays,
        int? nextDoseIntervalInDays)
    {
        if (protocolId == Guid.Empty)
        {
            return Result.Failure<VaccineProtocolDose>(ErrorCodes.VaccineProtocol.InvalidIdentifiers);
        }

        if (sequence <= 0)
        {
            return Result.Failure<VaccineProtocolDose>(ErrorCodes.VaccineProtocol.InvalidSequence);
        }

        if (string.IsNullOrWhiteSpace(label) || label.Length > 100)
        {
            return Result.Failure<VaccineProtocolDose>(ErrorCodes.VaccineProtocol.InvalidDoseLabel);
        }

        if (minAgeInDays < 0)
        {
            return Result.Failure<VaccineProtocolDose>(ErrorCodes.VaccineProtocol.InvalidAgeRange);
        }

        if (maxAgeInDays is not null && maxAgeInDays.Value < minAgeInDays)
        {
            return Result.Failure<VaccineProtocolDose>(ErrorCodes.VaccineProtocol.InvalidAgeRange);
        }

        return Result.Success(new VaccineProtocolDose(
            id,
            protocolId,
            sequence,
            label.Trim(),
            minAgeInDays,
            maxAgeInDays,
            intervalFromPreviousInDays,
            nextDoseIntervalInDays));
    }
}
