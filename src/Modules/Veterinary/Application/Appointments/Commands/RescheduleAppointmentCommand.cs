using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Application.Messaging;
using Core.Domain.Authorization;

namespace Veterinary.Application.Appointments.Commands;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.AppointmentsWrite)]
public record RescheduleAppointmentCommand(
    Guid AppointmentId,
    DateTimeOffset NewDate,
    DateTimeOffset? OccurredAt = null,
    Guid IdempotencyKey = default) : IIdempotentCommand;
