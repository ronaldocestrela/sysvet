using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Application.Messaging;
using Core.Domain.Authorization;
using Veterinary.Application.Hospitalizations.Dtos;

namespace Veterinary.Application.Hospitalizations.Commands;

/// <summary>Admits a pet to a bed.</summary>
[AuthorizeRequest(AuthorizationPolicies.Veterinarian, Permissions.HospitalizationsWrite)]
public sealed record AdmitPetCommand(
    Guid PetId,
    Guid VeterinarianId,
    Guid BedId,
    string Reason,
    Guid HospitalizationId = default,
    Guid IdempotencyKey = default)
    : IIdempotentCommand<Guid>;

/// <summary>Discharges an inpatient stay.</summary>
[AuthorizeRequest(AuthorizationPolicies.Veterinarian, Permissions.HospitalizationsWrite)]
public sealed record DischargePetCommand(Guid HospitalizationId, Guid IdempotencyKey = default)
    : IIdempotentCommand<bool>;

/// <summary>Moves patient to another bed.</summary>
[AuthorizeRequest(AuthorizationPolicies.Veterinarian, Permissions.HospitalizationsWrite)]
public sealed record TransferHospitalizationBedCommand(
    Guid HospitalizationId,
    Guid NewBedId,
    Guid IdempotencyKey = default)
    : IIdempotentCommand;

/// <summary>Creates a medication order with scheduled slots.</summary>
[AuthorizeRequest(AuthorizationPolicies.Veterinarian, Permissions.HospitalizationsWrite)]
public sealed record CreateMedicationOrderCommand(
    Guid HospitalizationId,
    string MedicationName,
    string Dose,
    string Route,
    IReadOnlyList<TimeOnly> DailyTimes,
    DateOnly StartsOn,
    DateOnly EndsOn,
    Guid OrderId = default,
    Guid IdempotencyKey = default)
    : IIdempotentCommand<Guid>;

/// <summary>Records administration of a scheduled dose.</summary>
[AuthorizeRequest(AuthorizationPolicies.Veterinarian, Permissions.HospitalizationsWrite)]
public sealed record AdministerMedicationCommand(
    Guid HospitalizationId,
    Guid AdministrationId,
    string? Notes,
    Guid IdempotencyKey = default)
    : IIdempotentCommand;

/// <summary>Skips a scheduled dose.</summary>
[AuthorizeRequest(AuthorizationPolicies.Veterinarian, Permissions.HospitalizationsWrite)]
public sealed record SkipMedicationCommand(
    Guid HospitalizationId,
    Guid AdministrationId,
    string? Notes,
    Guid IdempotencyKey = default)
    : IIdempotentCommand;

/// <summary>Adds daily evolution note.</summary>
[AuthorizeRequest(AuthorizationPolicies.Veterinarian, Permissions.HospitalizationsWrite)]
public sealed record AddHospitalizationProgressNoteCommand(
    Guid HospitalizationId,
    string Text,
    Guid NoteId = default,
    Guid IdempotencyKey = default)
    : IIdempotentCommand<Guid>;

/// <summary>Records inpatient procedure.</summary>
[AuthorizeRequest(AuthorizationPolicies.Veterinarian, Permissions.HospitalizationsWrite)]
public sealed record AddHospitalProcedureCommand(
    Guid HospitalizationId,
    string Name,
    Guid VeterinarianId,
    DateTimeOffset PerformedAt,
    string? Notes,
    Guid ProcedureId = default,
    Guid IdempotencyKey = default)
    : IIdempotentCommand<Guid>;

/// <summary>Gets hospitalization detail.</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.HospitalizationsRead)]
public sealed record GetHospitalizationByIdQuery(Guid HospitalizationId) : IQuery<HospitalizationDetailDto>;

/// <summary>Lists active hospitalizations.</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.HospitalizationsRead)]
public sealed record ListActiveHospitalizationsQuery : IQuery<IReadOnlyList<HospitalizationListItemDto>>;

/// <summary>Execution map for a UTC calendar day.</summary>
[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.HospitalizationsRead)]
public sealed record GetExecutionMapQuery(DateOnly? Date = null) : IQuery<ExecutionMapDto>;
