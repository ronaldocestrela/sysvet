using Core.Domain;
using Veterinary.Domain;

namespace Veterinary.Domain.ValueObjects;

/// <summary>
/// Snapshot of patient vital signs captured during a single consultation.
/// </summary>
public sealed class VitalSigns
{
    /// <summary>Body weight in kilograms.</summary>
    public decimal WeightKg { get; private set; }

    /// <summary>Body temperature in degrees Celsius.</summary>
    public decimal TemperatureC { get; private set; }

    /// <summary>Heart rate in beats per minute.</summary>
    public int? HeartRateBpm { get; private set; }

    /// <summary>Respiratory rate in breaths per minute.</summary>
    public int? RespiratoryRateBpm { get; private set; }

    /// <summary>Mucous membrane assessment (e.g. pink, pale).</summary>
    public string MucousMembranes { get; private set; } = string.Empty;

    /// <summary>Capillary refill time description.</summary>
    public string CapillaryRefillTime { get; private set; } = string.Empty;

    /// <summary>When the vitals were measured (UTC).</summary>
    public DateTimeOffset MeasuredAt { get; private set; }

    private VitalSigns() { }

    private VitalSigns(
        decimal weightKg,
        decimal temperatureC,
        int? heartRateBpm,
        int? respiratoryRateBpm,
        string mucousMembranes,
        string capillaryRefillTime,
        DateTimeOffset measuredAt)
    {
        WeightKg = weightKg;
        TemperatureC = temperatureC;
        HeartRateBpm = heartRateBpm;
        RespiratoryRateBpm = respiratoryRateBpm;
        MucousMembranes = mucousMembranes;
        CapillaryRefillTime = capillaryRefillTime;
        MeasuredAt = measuredAt;
    }

    /// <summary>Creates a validated vital signs snapshot for persistence on the medical record.</summary>
    public static Result<VitalSigns> Create(
        decimal weightKg,
        decimal temperatureC,
        int? heartRateBpm,
        int? respiratoryRateBpm,
        string mucousMembranes,
        string capillaryRefillTime,
        DateTimeOffset measuredAt)
    {
        if (weightKg <= 0m || weightKg > 500m)
        {
            return Result.Failure<VitalSigns>(ErrorCodes.MedicalRecord.InvalidVitals);
        }

        if (temperatureC < 30m || temperatureC > 45m)
        {
            return Result.Failure<VitalSigns>(ErrorCodes.MedicalRecord.InvalidVitals);
        }

        if (heartRateBpm is <= 0 or > 400)
        {
            return Result.Failure<VitalSigns>(ErrorCodes.MedicalRecord.InvalidVitals);
        }

        if (respiratoryRateBpm is <= 0 or > 200)
        {
            return Result.Failure<VitalSigns>(ErrorCodes.MedicalRecord.InvalidVitals);
        }

        if (mucousMembranes.Length > 200)
        {
            return Result.Failure<VitalSigns>(ErrorCodes.MedicalRecord.InvalidVitals);
        }

        if (capillaryRefillTime.Length > 200)
        {
            return Result.Failure<VitalSigns>(ErrorCodes.MedicalRecord.InvalidVitals);
        }

        if (measuredAt > DateTimeOffset.UtcNow.AddMinutes(5))
        {
            return Result.Failure<VitalSigns>(ErrorCodes.MedicalRecord.InvalidVitals);
        }

        return Result.Success(new VitalSigns(
            weightKg,
            temperatureC,
            heartRateBpm,
            respiratoryRateBpm,
            mucousMembranes.Trim(),
            capillaryRefillTime.Trim(),
            measuredAt));
    }
}
