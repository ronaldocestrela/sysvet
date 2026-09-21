using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Application.Messaging;
using Core.Domain.Authorization;

namespace Petshop.Application.GroomingAppointments.Commands;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.GroomingWrite)]
public record ScheduleGroomingAppointmentCommand(
    Guid Id,
    Guid TutorId,
    Guid PetId,
    Guid GroomerId,
    Guid GroomingServiceId,
    DateTimeOffset Date,
    int DurationInMinutes,
    string Notes,
    Guid IdempotencyKey = default) : IIdempotentCommand<Guid>;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.GroomingWrite)]
public record ConfirmGroomingAppointmentCommand(Guid GroomingAppointmentId, Guid IdempotencyKey = default) : IIdempotentCommand;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.GroomingWrite)]
public record StartGroomingAppointmentCommand(Guid GroomingAppointmentId, Guid IdempotencyKey = default) : IIdempotentCommand;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.GroomingWrite)]
public record CompleteGroomingAppointmentCommand(Guid GroomingAppointmentId, Guid IdempotencyKey = default) : IIdempotentCommand;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.GroomingWrite)]
public record CancelGroomingAppointmentCommand(Guid GroomingAppointmentId, Guid IdempotencyKey = default) : IIdempotentCommand;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.GroomingWrite)]
public record MarkNoShowGroomingAppointmentCommand(Guid GroomingAppointmentId, Guid IdempotencyKey = default) : IIdempotentCommand;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.GroomingWrite)]
public record RescheduleGroomingAppointmentCommand(Guid GroomingAppointmentId, DateTimeOffset NewDate, Guid IdempotencyKey = default) : IIdempotentCommand;
