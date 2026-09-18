using Core.Domain;
using Core.Domain.Entities;
using Veterinary.Domain;

namespace Veterinary.Domain.Entities;

/// <summary>Tenant-scoped vaccine protocol catalog filtered by species (and age via dose lines).</summary>
public sealed class VaccineProtocol : AggregateRoot
{
    private readonly List<VaccineProtocolDose> _doses = new();

    /// <summary>Display name for staff selection.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Target species for this protocol.</summary>
    public PetSpecies Species { get; private set; }

    /// <summary>Whether the protocol appears in pick lists.</summary>
    public bool IsActive { get; private set; }

    /// <summary>Ordered dose definitions.</summary>
    public IReadOnlyCollection<VaccineProtocolDose> Doses => _doses.AsReadOnly();

    private VaccineProtocol() { }

    private VaccineProtocol(Guid id, string name, PetSpecies species)
        : base(id)
    {
        Name = name;
        Species = species;
        IsActive = true;
    }

    /// <summary>Creates a new active protocol.</summary>
    public static Result<VaccineProtocol> Create(Guid id, string name, PetSpecies species)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 200)
        {
            return Result.Failure<VaccineProtocol>(ErrorCodes.VaccineProtocol.InvalidName);
        }

        if (!Enum.IsDefined(typeof(PetSpecies), species))
        {
            return Result.Failure<VaccineProtocol>(ErrorCodes.VaccineProtocol.InvalidSpecies);
        }

        var protocol = new VaccineProtocol(id, name.Trim(), species);
        protocol.Touch();
        return Result.Success(protocol);
    }

    /// <summary>Rehydrates from sync without validation.</summary>
    public static VaccineProtocol RestoreFromSync(
        Guid id,
        string name,
        PetSpecies species,
        bool isActive,
        DateTimeOffset updatedAt,
        IEnumerable<(Guid DoseId, int Sequence, string Label, int MinAgeInDays, int? MaxAgeInDays, int? IntervalFromPreviousInDays, int? NextDoseIntervalInDays)> doses)
    {
        var protocol = new VaccineProtocol(id, name, species)
        {
            IsActive = isActive,
            UpdatedAt = updatedAt
        };

        foreach (var dose in doses)
        {
            var created = VaccineProtocolDose.Create(
                dose.DoseId,
                id,
                dose.Sequence,
                dose.Label,
                dose.MinAgeInDays,
                dose.MaxAgeInDays,
                dose.IntervalFromPreviousInDays,
                dose.NextDoseIntervalInDays);
            if (created.IsSuccess)
            {
                protocol._doses.Add(created.Value);
            }
        }

        return protocol;
    }

    /// <summary>Applies remote sync snapshot (LWW).</summary>
    public void ApplySyncSnapshot(
        string name,
        PetSpecies species,
        bool isActive,
        DateTimeOffset updatedAt,
        IEnumerable<(Guid DoseId, int Sequence, string Label, int MinAgeInDays, int? MaxAgeInDays, int? IntervalFromPreviousInDays, int? NextDoseIntervalInDays)> doses)
    {
        Name = name;
        Species = species;
        IsActive = isActive;
        UpdatedAt = updatedAt;
        _doses.Clear();

        foreach (var dose in doses)
        {
            var created = VaccineProtocolDose.Create(
                dose.DoseId,
                Id,
                dose.Sequence,
                dose.Label,
                dose.MinAgeInDays,
                dose.MaxAgeInDays,
                dose.IntervalFromPreviousInDays,
                dose.NextDoseIntervalInDays);
            if (created.IsSuccess)
            {
                _doses.Add(created.Value);
            }
        }
    }

    /// <summary>Renames the protocol.</summary>
    public Result UpdateDetails(string name, PetSpecies species)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 200)
        {
            return Result.Failure(ErrorCodes.VaccineProtocol.InvalidName);
        }

        if (!Enum.IsDefined(typeof(PetSpecies), species))
        {
            return Result.Failure(ErrorCodes.VaccineProtocol.InvalidSpecies);
        }

        Name = name.Trim();
        Species = species;
        Touch();
        return Result.Success();
    }

    /// <summary>Replaces all dose lines (full snapshot).</summary>
    public Result ReplaceDoses(IEnumerable<(Guid DoseId, string Label, int MinAgeInDays, int? MaxAgeInDays, int? IntervalFromPreviousInDays, int? NextDoseIntervalInDays)> doses)
    {
        _doses.Clear();
        var sequence = 1;
        foreach (var dose in doses)
        {
            var doseId = dose.DoseId == Guid.Empty ? Guid.NewGuid() : dose.DoseId;
            var created = VaccineProtocolDose.Create(
                doseId,
                Id,
                sequence++,
                dose.Label,
                dose.MinAgeInDays,
                dose.MaxAgeInDays,
                dose.IntervalFromPreviousInDays,
                dose.NextDoseIntervalInDays);
            if (created.IsFailure)
            {
                return Result.Failure(created.Error);
            }

            _doses.Add(created.Value);
        }

        Touch();
        return Result.Success();
    }

    /// <summary>Marks the protocol inactive.</summary>
    public Result Deactivate()
    {
        if (!IsActive)
        {
            return Result.Failure(ErrorCodes.VaccineProtocol.AlreadyInactive);
        }

        IsActive = false;
        Touch();
        return Result.Success();
    }

    private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
}
