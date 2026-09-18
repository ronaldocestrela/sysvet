using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Application.Messaging;
using Core.Domain.Authorization;

namespace Veterinary.Application.MedicalRecords.Commands;

/// <summary>Opens or returns the medical record linked to an appointment (idempotent).</summary>
[AuthorizeRequest(AuthorizationPolicies.Veterinarian, Permissions.MedicalRecordsWrite)]
public record CreateMedicalRecordCommand(Guid AppointmentId, Guid IdempotencyKey = default) : IIdempotentCommand<Guid>;
