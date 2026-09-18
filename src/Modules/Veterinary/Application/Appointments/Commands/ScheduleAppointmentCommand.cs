using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Application.Messaging;
using Core.Domain.Authorization;

namespace Veterinary.Application.Appointments.Commands;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.AppointmentsWrite)]
public record ScheduleAppointmentCommand(
    Guid Id,
    Guid TutorId,
    Guid PetId,
    Guid VeterinarianId,
    DateTimeOffset Date,
    int DurationInMinutes,
    string Reason,
    Guid IdempotencyKey = default) : IIdempotentCommand<Guid>;
