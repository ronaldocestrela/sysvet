using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Application.Messaging;
using Core.Domain.Authorization;

namespace Veterinary.Application.MedicalRecords.Commands;

/// <summary>Replaces anamnesis on a draft medical record.</summary>
[AuthorizeRequest(AuthorizationPolicies.Veterinarian, Permissions.MedicalRecordsWrite)]
public record UpdateAnamnesisCommand(Guid MedicalRecordId, string Anamnesis, Guid IdempotencyKey = default) : IIdempotentCommand;

/// <summary>Replaces vital signs snapshot on a draft medical record.</summary>
[AuthorizeRequest(AuthorizationPolicies.Veterinarian, Permissions.MedicalRecordsWrite)]
public record RecordVitalSignsCommand(
    Guid MedicalRecordId,
    decimal WeightKg,
    decimal TemperatureC,
    int? HeartRateBpm,
    int? RespiratoryRateBpm,
    string MucousMembranes,
    string CapillaryRefillTime,
    DateTimeOffset MeasuredAt,
    Guid IdempotencyKey = default) : IIdempotentCommand;

/// <summary>Appends an evolution note to a draft medical record.</summary>
[AuthorizeRequest(AuthorizationPolicies.Veterinarian, Permissions.MedicalRecordsWrite)]
public record AddEvolutionNoteCommand(
    Guid MedicalRecordId,
    string Text,
    Guid NoteId = default,
    DateTimeOffset? RecordedAt = null,
    Guid IdempotencyKey = default) : IIdempotentCommand<Guid>;

/// <summary>Replaces diagnosis on a draft medical record.</summary>
[AuthorizeRequest(AuthorizationPolicies.Veterinarian, Permissions.MedicalRecordsWrite)]
public record SetDiagnosisCommand(Guid MedicalRecordId, string Diagnosis, Guid IdempotencyKey = default) : IIdempotentCommand;

/// <summary>Replaces conduct/plan on a draft medical record.</summary>
[AuthorizeRequest(AuthorizationPolicies.Veterinarian, Permissions.MedicalRecordsWrite)]
public record SetConductCommand(Guid MedicalRecordId, string Conduct, Guid IdempotencyKey = default) : IIdempotentCommand;

/// <summary>Finalizes a medical record (immutable afterward).</summary>
[AuthorizeRequest(AuthorizationPolicies.Veterinarian, Permissions.MedicalRecordsWrite)]
public record FinalizeMedicalRecordCommand(Guid MedicalRecordId, Guid IdempotencyKey = default) : IIdempotentCommand;
