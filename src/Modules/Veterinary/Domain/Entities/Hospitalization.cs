using Core.Domain;
using Veterinary.Domain;
using Veterinary.Domain.Enums;
using Veterinary.Domain.Services;

namespace Veterinary.Domain.Entities;

/// <summary>Inpatient stay aggregate with medication schedule and clinical timeline.</summary>
public enum HospitalizationStatus
{
    Admitted = 1,
    Discharged = 2
}

public class Hospitalization : AggregateRoot
{
    private readonly List<HospitalMedicationOrder> _medicationOrders = new();
    private readonly List<MedicationAdministration> _administrations = new();
    private readonly List<HospitalizationProgressNote> _progressNotes = new();
    private readonly List<HospitalProcedure> _procedures = new();

    /// <summary>Patient pet.</summary>
    public Guid PetId { get; private set; }

    /// <summary>Attending veterinarian at admission.</summary>
    public Guid VeterinarianId { get; private set; }

    /// <summary>Assigned bed.</summary>
    public Guid BedId { get; private set; }

    /// <summary>Admission reason.</summary>
    public string Reason { get; private set; }

    /// <summary>When admitted (UTC).</summary>
    public DateTimeOffset AdmittedAt { get; private set; }

    /// <summary>When discharged, if applicable.</summary>
    public DateTimeOffset? DischargedAt { get; private set; }

    /// <summary>Stay status.</summary>
    public HospitalizationStatus Status { get; private set; }

    /// <summary>Medication orders for this stay.</summary>
    public IReadOnlyCollection<HospitalMedicationOrder> MedicationOrders => _medicationOrders.AsReadOnly();

    /// <summary>Expanded administration slots.</summary>
    public IReadOnlyCollection<MedicationAdministration> Administrations => _administrations.AsReadOnly();

    /// <summary>Daily evolution notes.</summary>
    public IReadOnlyCollection<HospitalizationProgressNote> ProgressNotes => _progressNotes.AsReadOnly();

    /// <summary>Procedures performed during stay.</summary>
    public IReadOnlyCollection<HospitalProcedure> Procedures => _procedures.AsReadOnly();

    private Hospitalization()
    {
        Reason = string.Empty;
    }

    private Hospitalization(Guid id, Guid petId, Guid veterinarianId, Guid bedId, string reason, DateTimeOffset admittedAt)
        : base(id)
    {
        PetId = petId;
        VeterinarianId = veterinarianId;
        BedId = bedId;
        Reason = reason;
        AdmittedAt = admittedAt;
        Status = HospitalizationStatus.Admitted;
    }

    /// <summary>Admits a pet to a bed.</summary>
    public static Result<Hospitalization> Admit(
        Guid id,
        Guid petId,
        Guid veterinarianId,
        Guid bedId,
        string reason,
        DateTimeOffset admittedAt)
    {
        if (petId == Guid.Empty || veterinarianId == Guid.Empty || bedId == Guid.Empty)
        {
            return Result.Failure<Hospitalization>(ErrorCodes.Hospitalization.InvalidIdentifiers);
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            return Result.Failure<Hospitalization>(ErrorCodes.Hospitalization.InvalidReason);
        }

        var hosp = new Hospitalization(id, petId, veterinarianId, bedId, reason.Trim(), admittedAt);
        hosp.Touch();
        return Result.Success(hosp);
    }

    /// <summary>Rehydrates from sync pull.</summary>
    public static Hospitalization RestoreFromSync(
        Guid id,
        Guid petId,
        Guid veterinarianId,
        Guid bedId,
        string reason,
        DateTimeOffset admittedAt,
        DateTimeOffset? dischargedAt,
        HospitalizationStatus status,
        DateTimeOffset updatedAt,
        IEnumerable<(Guid OrderId, string MedicationName, string Dose, string Route, string DailyTimesCsv, DateOnly StartsOn, DateOnly EndsOn, HospitalMedicationOrderStatus OrderStatus, DateTimeOffset OrderUpdatedAt)> orders,
        IEnumerable<(Guid AdminId, Guid OrderId, DateTimeOffset ScheduledAt, MedicationAdministrationStatus AdminStatus, Guid? ActorId, DateTimeOffset? ActedAt, string Notes, DateTimeOffset AdminUpdatedAt)> administrations,
        IEnumerable<(Guid NoteId, Guid AuthorId, string Text, DateTimeOffset RecordedAt, DateTimeOffset NoteUpdatedAt)> notes,
        IEnumerable<(Guid ProcedureId, string Name, Guid VetId, DateTimeOffset PerformedAt, string ProcedureNotes, DateTimeOffset ProcedureUpdatedAt)> procedures)
    {
        var hosp = new Hospitalization(id, petId, veterinarianId, bedId, reason, admittedAt)
        {
            DischargedAt = dischargedAt,
            Status = status,
            UpdatedAt = updatedAt
        };

        foreach (var order in orders)
        {
            hosp._medicationOrders.Add(HospitalMedicationOrder.RestoreFromSync(
                order.OrderId,
                id,
                order.MedicationName,
                order.Dose,
                order.Route,
                order.DailyTimesCsv,
                order.StartsOn,
                order.EndsOn,
                order.OrderStatus,
                order.OrderUpdatedAt));
        }

        foreach (var admin in administrations)
        {
            hosp._administrations.Add(MedicationAdministration.RestoreFromSync(
                admin.AdminId,
                id,
                admin.OrderId,
                admin.ScheduledAt,
                admin.AdminStatus,
                admin.ActorId,
                admin.ActedAt,
                admin.Notes,
                admin.AdminUpdatedAt));
        }

        foreach (var note in notes)
        {
            hosp._progressNotes.Add(HospitalizationProgressNote.RestoreFromSync(
                note.NoteId,
                id,
                note.AuthorId,
                note.Text,
                note.RecordedAt,
                note.NoteUpdatedAt));
        }

        foreach (var proc in procedures)
        {
            hosp._procedures.Add(HospitalProcedure.RestoreFromSync(
                proc.ProcedureId,
                id,
                proc.Name,
                proc.VetId,
                proc.PerformedAt,
                proc.ProcedureNotes,
                proc.ProcedureUpdatedAt));
        }

        return hosp;
    }

    /// <summary>Applies remote sync snapshot (LWW).</summary>
    public void ApplySyncSnapshot(
        Guid petId,
        Guid veterinarianId,
        Guid bedId,
        string reason,
        DateTimeOffset admittedAt,
        DateTimeOffset? dischargedAt,
        HospitalizationStatus status,
        DateTimeOffset updatedAt,
        IEnumerable<(Guid OrderId, string MedicationName, string Dose, string Route, string DailyTimesCsv, DateOnly StartsOn, DateOnly EndsOn, HospitalMedicationOrderStatus OrderStatus, DateTimeOffset OrderUpdatedAt)> orders,
        IEnumerable<(Guid AdminId, Guid OrderId, DateTimeOffset ScheduledAt, MedicationAdministrationStatus AdminStatus, Guid? ActorId, DateTimeOffset? ActedAt, string Notes, DateTimeOffset AdminUpdatedAt)> administrations,
        IEnumerable<(Guid NoteId, Guid AuthorId, string Text, DateTimeOffset RecordedAt, DateTimeOffset NoteUpdatedAt)> notes,
        IEnumerable<(Guid ProcedureId, string Name, Guid VetId, DateTimeOffset PerformedAt, string ProcedureNotes, DateTimeOffset ProcedureUpdatedAt)> procedures)
    {
        PetId = petId;
        VeterinarianId = veterinarianId;
        BedId = bedId;
        Reason = reason;
        AdmittedAt = admittedAt;
        DischargedAt = dischargedAt;
        Status = status;
        UpdatedAt = updatedAt;
        _medicationOrders.Clear();
        _administrations.Clear();
        _progressNotes.Clear();
        _procedures.Clear();

        foreach (var order in orders)
        {
            _medicationOrders.Add(HospitalMedicationOrder.RestoreFromSync(
                order.OrderId,
                Id,
                order.MedicationName,
                order.Dose,
                order.Route,
                order.DailyTimesCsv,
                order.StartsOn,
                order.EndsOn,
                order.OrderStatus,
                order.OrderUpdatedAt));
        }

        foreach (var admin in administrations)
        {
            _administrations.Add(MedicationAdministration.RestoreFromSync(
                admin.AdminId,
                Id,
                admin.OrderId,
                admin.ScheduledAt,
                admin.AdminStatus,
                admin.ActorId,
                admin.ActedAt,
                admin.Notes,
                admin.AdminUpdatedAt));
        }

        foreach (var note in notes)
        {
            _progressNotes.Add(HospitalizationProgressNote.RestoreFromSync(
                note.NoteId,
                Id,
                note.AuthorId,
                note.Text,
                note.RecordedAt,
                note.NoteUpdatedAt));
        }

        foreach (var proc in procedures)
        {
            _procedures.Add(HospitalProcedure.RestoreFromSync(
                proc.ProcedureId,
                Id,
                proc.Name,
                proc.VetId,
                proc.PerformedAt,
                proc.ProcedureNotes,
                proc.ProcedureUpdatedAt));
        }
    }

    /// <summary>Moves patient to another bed while admitted.</summary>
    public Result TransferBed(Guid newBedId)
    {
        if (!EnsureAdmitted())
        {
            return Result.Failure(ErrorCodes.Hospitalization.Discharged);
        }

        if (newBedId == Guid.Empty || newBedId == BedId)
        {
            return Result.Failure(ErrorCodes.Hospitalization.InvalidBed);
        }

        BedId = newBedId;
        Touch();
        return Result.Success();
    }

    /// <summary>Discharges the patient and cancels pending administrations.</summary>
    public Result<bool> Discharge(DateTimeOffset dischargedAt)
    {
        if (Status == HospitalizationStatus.Discharged)
        {
            return Result.Failure<bool>(ErrorCodes.Hospitalization.AlreadyDischarged);
        }

        Status = HospitalizationStatus.Discharged;
        DischargedAt = dischargedAt;
        foreach (var admin in _administrations)
        {
            admin.CancelPending(dischargedAt);
        }

        Touch();
        return Result.Success(true);
    }

    /// <summary>Adds a medication order and materializes administration slots.</summary>
    public Result<Guid> AddMedicationOrder(
        Guid orderId,
        string medicationName,
        string dose,
        string route,
        IReadOnlyList<TimeOnly> dailyTimes,
        DateOnly startsOn,
        DateOnly endsOn)
    {
        if (!EnsureAdmitted())
        {
            return Result.Failure<Guid>(ErrorCodes.Hospitalization.Discharged);
        }

        var orderResult = HospitalMedicationOrder.Create(orderId, Id, medicationName, dose, route, dailyTimes, startsOn, endsOn);
        if (orderResult.IsFailure)
        {
            return Result.Failure<Guid>(orderResult.Error);
        }

        var order = orderResult.Value;
        _medicationOrders.Add(order);

        var occurrences = MedicationSchedule.ExpandOccurrences(startsOn, endsOn, dailyTimes);
        foreach (var scheduledAt in occurrences)
        {
            var adminId = Guid.NewGuid();
            var admin = MedicationAdministration.CreatePending(adminId, Id, order.Id, scheduledAt);
            if (admin.IsSuccess)
            {
                _administrations.Add(admin.Value);
            }
        }

        Touch();
        return Result.Success(order.Id);
    }

    /// <summary>Records administration of a scheduled dose.</summary>
    public Result AdministerMedication(Guid administrationId, Guid actorId, string? notes, DateTimeOffset actedAt)
    {
        if (!EnsureAdmitted())
        {
            return Result.Failure(ErrorCodes.Hospitalization.Discharged);
        }

        var admin = _administrations.FirstOrDefault(a => a.Id == administrationId);
        if (admin is null)
        {
            return Result.Failure(ErrorCodes.MedicationAdministration.NotFound);
        }

        var result = admin.Administer(actorId, notes, actedAt);
        if (result.IsFailure)
        {
            return result;
        }

        Touch();
        return Result.Success();
    }

    /// <summary>Skips a scheduled dose.</summary>
    public Result SkipMedication(Guid administrationId, Guid actorId, string? notes, DateTimeOffset actedAt)
    {
        if (!EnsureAdmitted())
        {
            return Result.Failure(ErrorCodes.Hospitalization.Discharged);
        }

        var admin = _administrations.FirstOrDefault(a => a.Id == administrationId);
        if (admin is null)
        {
            return Result.Failure(ErrorCodes.MedicationAdministration.NotFound);
        }

        var result = admin.Skip(actorId, notes, actedAt);
        if (result.IsFailure)
        {
            return result;
        }

        Touch();
        return Result.Success();
    }

    /// <summary>Appends a daily evolution note.</summary>
    public Result AddProgressNote(Guid noteId, Guid authorId, string text, DateTimeOffset recordedAt)
    {
        if (!EnsureAdmitted())
        {
            return Result.Failure(ErrorCodes.Hospitalization.Discharged);
        }

        var noteResult = HospitalizationProgressNote.Create(noteId, Id, authorId, text, recordedAt);
        if (noteResult.IsFailure)
        {
            return noteResult;
        }

        _progressNotes.Add(noteResult.Value);
        Touch();
        return Result.Success();
    }

    /// <summary>Records an inpatient procedure.</summary>
    public Result AddProcedure(Guid procedureId, string name, Guid veterinarianId, DateTimeOffset performedAt, string? notes)
    {
        if (!EnsureAdmitted())
        {
            return Result.Failure(ErrorCodes.Hospitalization.Discharged);
        }

        var procResult = HospitalProcedure.Create(procedureId, Id, name, veterinarianId, performedAt, notes);
        if (procResult.IsFailure)
        {
            return procResult;
        }

        _procedures.Add(procResult.Value);
        Touch();
        return Result.Success();
    }

    private bool EnsureAdmitted() => Status == HospitalizationStatus.Admitted;

    private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
}
