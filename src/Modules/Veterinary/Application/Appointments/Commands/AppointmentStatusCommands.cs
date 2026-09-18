using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Application.Messaging;
using Core.Domain.Authorization;

namespace Veterinary.Application.Appointments.Commands;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.AppointmentsWrite)]
public record ConfirmAppointmentCommand(Guid AppointmentId, Guid IdempotencyKey = default) : IIdempotentCommand;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.AppointmentsWrite)]
public record StartAppointmentCommand(Guid AppointmentId, Guid IdempotencyKey = default) : IIdempotentCommand;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.AppointmentsWrite)]
public record CompleteAppointmentCommand(Guid AppointmentId, Guid IdempotencyKey = default) : IIdempotentCommand;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.AppointmentsWrite)]
public record MarkNoShowAppointmentCommand(Guid AppointmentId, Guid IdempotencyKey = default) : IIdempotentCommand;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.AppointmentsWrite)]
public record CancelAppointmentCommand(Guid AppointmentId, Guid IdempotencyKey = default) : IIdempotentCommand;
