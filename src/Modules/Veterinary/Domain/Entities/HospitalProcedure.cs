using Core.Domain;
using Veterinary.Domain;

namespace Veterinary.Domain.Entities;

/// <summary>Append-only inpatient procedure record.</summary>
public sealed class HospitalProcedure : Entity
{
    /// <summary>Parent hospitalization.</summary>
    public Guid HospitalizationId { get; private set; }

    /// <summary>Procedure name.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Responsible veterinarian.</summary>
    public Guid VeterinarianId { get; private set; }

    /// <summary>When performed (UTC).</summary>
    public DateTimeOffset PerformedAt { get; private set; }

    /// <summary>Optional notes.</summary>
    public string Notes { get; private set; } = string.Empty;

    private HospitalProcedure() { }

    private HospitalProcedure(Guid id, Guid hospitalizationId, string name, Guid veterinarianId, DateTimeOffset performedAt, string notes)
        : base(id)
    {
        HospitalizationId = hospitalizationId;
        Name = name;
        VeterinarianId = veterinarianId;
        PerformedAt = performedAt;
        Notes = notes;
    }

    /// <summary>Creates a procedure entry.</summary>
    internal static Result<HospitalProcedure> Create(
        Guid id,
        Guid hospitalizationId,
        string name,
        Guid veterinarianId,
        DateTimeOffset performedAt,
        string? notes)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 200)
        {
            return Result.Failure<HospitalProcedure>(ErrorCodes.HospitalProcedure.InvalidName);
        }

        if (veterinarianId == Guid.Empty)
        {
            return Result.Failure<HospitalProcedure>(ErrorCodes.HospitalProcedure.InvalidIdentifiers);
        }

        return Result.Success(new HospitalProcedure(
            id,
            hospitalizationId,
            name.Trim(),
            veterinarianId,
            performedAt,
            notes?.Trim() ?? string.Empty));
    }

    /// <summary>Rehydrates from sync.</summary>
    internal static HospitalProcedure RestoreFromSync(
        Guid id,
        Guid hospitalizationId,
        string name,
        Guid veterinarianId,
        DateTimeOffset performedAt,
        string notes,
        DateTimeOffset updatedAt)
    {
        return new HospitalProcedure(id, hospitalizationId, name, veterinarianId, performedAt, notes) { UpdatedAt = updatedAt };
    }
}
