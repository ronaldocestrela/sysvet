using Core.Domain;
using Veterinary.Domain;
using Veterinary.Domain.Enums;
using Veterinary.Domain.Services;

namespace Veterinary.Domain.Entities;

/// <summary>Scheduled inpatient medication order with daily time slots.</summary>
public sealed class HospitalMedicationOrder : Entity
{
    /// <summary>Parent hospitalization.</summary>
    public Guid HospitalizationId { get; private set; }

    /// <summary>Medication display name.</summary>
    public string MedicationName { get; private set; } = string.Empty;

    /// <summary>Dose description (e.g. 500mg).</summary>
    public string Dose { get; private set; } = string.Empty;

    /// <summary>Route (e.g. IV, PO).</summary>
    public string Route { get; private set; } = string.Empty;

    /// <summary>Comma-separated HH:mm times in UTC for each day.</summary>
    public string DailyTimesCsv { get; private set; } = string.Empty;

    /// <summary>First calendar day (UTC) the order is active.</summary>
    public DateOnly StartsOn { get; private set; }

    /// <summary>Last calendar day (UTC) the order is active.</summary>
    public DateOnly EndsOn { get; private set; }

    /// <summary>Order workflow status.</summary>
    public HospitalMedicationOrderStatus Status { get; private set; }

    private HospitalMedicationOrder() { }

    private HospitalMedicationOrder(
        Guid id,
        Guid hospitalizationId,
        string medicationName,
        string dose,
        string route,
        string dailyTimesCsv,
        DateOnly startsOn,
        DateOnly endsOn)
        : base(id)
    {
        HospitalizationId = hospitalizationId;
        MedicationName = medicationName;
        Dose = dose;
        Route = route;
        DailyTimesCsv = dailyTimesCsv;
        StartsOn = startsOn;
        EndsOn = endsOn;
        Status = HospitalMedicationOrderStatus.Active;
    }

    /// <summary>Parsed daily administration times.</summary>
    public IReadOnlyList<TimeOnly> DailyTimes => ParseDailyTimes(DailyTimesCsv);

    /// <summary>Creates a new active medication order.</summary>
    public static Result<HospitalMedicationOrder> Create(
        Guid id,
        Guid hospitalizationId,
        string medicationName,
        string dose,
        string route,
        IReadOnlyList<TimeOnly> dailyTimes,
        DateOnly startsOn,
        DateOnly endsOn)
    {
        if (hospitalizationId == Guid.Empty)
        {
            return Result.Failure<HospitalMedicationOrder>(ErrorCodes.Hospitalization.InvalidIdentifiers);
        }

        if (string.IsNullOrWhiteSpace(medicationName) || medicationName.Length > 200)
        {
            return Result.Failure<HospitalMedicationOrder>(ErrorCodes.HospitalMedicationOrder.InvalidMedication);
        }

        if (dailyTimes.Count == 0)
        {
            return Result.Failure<HospitalMedicationOrder>(ErrorCodes.HospitalMedicationOrder.InvalidSchedule);
        }

        if (endsOn < startsOn)
        {
            return Result.Failure<HospitalMedicationOrder>(ErrorCodes.HospitalMedicationOrder.InvalidSchedule);
        }

        var spanDays = endsOn.DayNumber - startsOn.DayNumber + 1;
        if (spanDays > MedicationSchedule.MaxOrderDays)
        {
            return Result.Failure<HospitalMedicationOrder>(ErrorCodes.HospitalMedicationOrder.InvalidSchedule);
        }

        var csv = SerializeDailyTimes(dailyTimes);
        return Result.Success(new HospitalMedicationOrder(
            id,
            hospitalizationId,
            medicationName.Trim(),
            dose?.Trim() ?? string.Empty,
            route?.Trim() ?? string.Empty,
            csv,
            startsOn,
            endsOn));
    }

    /// <summary>Rehydrates from sync.</summary>
    internal static HospitalMedicationOrder RestoreFromSync(
        Guid id,
        Guid hospitalizationId,
        string medicationName,
        string dose,
        string route,
        string dailyTimesCsv,
        DateOnly startsOn,
        DateOnly endsOn,
        HospitalMedicationOrderStatus status,
        DateTimeOffset updatedAt)
    {
        var order = new HospitalMedicationOrder(id, hospitalizationId, medicationName, dose, route, dailyTimesCsv, startsOn, endsOn)
        {
            Status = status,
            UpdatedAt = updatedAt
        };
        return order;
    }

    internal static string SerializeDailyTimes(IReadOnlyList<TimeOnly> times) =>
        string.Join(',', times.OrderBy(t => t).Select(t => t.ToString("HH\\:mm")));

    internal static IReadOnlyList<TimeOnly> ParseDailyTimes(string csv)
    {
        if (string.IsNullOrWhiteSpace(csv))
        {
            return Array.Empty<TimeOnly>();
        }

        return csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(TimeOnly.Parse)
            .ToList();
    }
}
