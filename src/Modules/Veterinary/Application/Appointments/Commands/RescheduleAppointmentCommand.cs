using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;

namespace Veterinary.Application.Appointments.Commands;

[AuthorizeRequest(AuthorizationPolicies.Veterinarian)]
public record RescheduleAppointmentCommand(
    Guid AppointmentId,
    DateTimeOffset NewDate) : ICommand;
