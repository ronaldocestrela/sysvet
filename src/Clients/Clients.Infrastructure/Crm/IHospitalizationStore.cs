using Core.Domain;

namespace Clients.Infrastructure.Crm;

/// <summary>Offline-first hospitalization and execution map.</summary>
public interface IHospitalizationStore
{
    Task<Result<ExecutionMapViewModel>> GetExecutionMapAsync(DateOnly? date = null, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<HospitalizationListViewModel>>> ListActiveAsync(CancellationToken cancellationToken = default);

    Task<Result<Guid>> AdmitAsync(Guid petId, Guid veterinarianId, Guid bedId, string reason, CancellationToken cancellationToken = default);

    Task<Result> DischargeAsync(Guid hospitalizationId, CancellationToken cancellationToken = default);

    Task<Result> AdministerAsync(Guid hospitalizationId, Guid administrationId, string? notes, CancellationToken cancellationToken = default);

    Task<Result> SkipAsync(Guid hospitalizationId, Guid administrationId, string? notes, CancellationToken cancellationToken = default);

    Task<Result<Guid>> CreateMedicationOrderAsync(
        Guid hospitalizationId,
        string medicationName,
        string dose,
        string route,
        IReadOnlyList<TimeOnly> dailyTimes,
        DateOnly startsOn,
        DateOnly endsOn,
        CancellationToken cancellationToken = default);

    Task<Result<HospitalizationDetailViewModel>> GetDetailAsync(Guid hospitalizationId, CancellationToken cancellationToken = default);
}

/// <summary>Inpatient stay detail for the client UI.</summary>
public sealed class HospitalizationDetailViewModel
{
    public Guid Id { get; init; }
    public Guid PetId { get; init; }
    public string PetName { get; init; } = string.Empty;
    public Guid BedId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public DateTimeOffset AdmittedAt { get; init; }
    public string Status { get; init; } = string.Empty;
    public IReadOnlyList<MedicationOrderViewModel> MedicationOrders { get; init; } = Array.Empty<MedicationOrderViewModel>();
    public IReadOnlyList<ProgressNoteViewModel> ProgressNotes { get; init; } = Array.Empty<ProgressNoteViewModel>();
    public IReadOnlyList<HospitalProcedureViewModel> Procedures { get; init; } = Array.Empty<HospitalProcedureViewModel>();
}

public sealed class MedicationOrderViewModel
{
    public Guid Id { get; init; }
    public string MedicationName { get; init; } = string.Empty;
    public string Dose { get; init; } = string.Empty;
    public string Route { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
}

public sealed class ProgressNoteViewModel
{
    public Guid Id { get; init; }
    public string Text { get; init; } = string.Empty;
    public DateTimeOffset RecordedAt { get; init; }
}

public sealed class HospitalProcedureViewModel
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public DateTimeOffset PerformedAt { get; init; }
}

public sealed class HospitalizationListViewModel
{
    public Guid Id { get; init; }
    public Guid PetId { get; init; }
    public string PetName { get; init; } = string.Empty;
    public Guid BedId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public DateTimeOffset AdmittedAt { get; init; }
}

public sealed class ExecutionMapViewModel
{
    public DateOnly Date { get; init; }
    public IReadOnlyList<ExecutionMapWardViewModel> Wards { get; init; } = Array.Empty<ExecutionMapWardViewModel>();
}

public sealed class ExecutionMapWardViewModel
{
    public Guid WardUnitId { get; init; }
    public string WardName { get; init; } = string.Empty;
    public IReadOnlyList<ExecutionMapBedViewModel> Beds { get; init; } = Array.Empty<ExecutionMapBedViewModel>();
}

public sealed class ExecutionMapBedViewModel
{
    public Guid BedId { get; init; }
    public string BedCode { get; init; } = string.Empty;
    public Guid? HospitalizationId { get; init; }
    public string PetName { get; init; } = string.Empty;
    public IReadOnlyList<ExecutionMapAdministrationViewModel> Administrations { get; init; } = Array.Empty<ExecutionMapAdministrationViewModel>();
}

public sealed class ExecutionMapAdministrationViewModel
{
    public Guid Id { get; init; }
    public string MedicationName { get; init; } = string.Empty;
    public string Dose { get; init; } = string.Empty;
    public DateTimeOffset ScheduledAt { get; init; }
    public string Status { get; init; } = string.Empty;
}
