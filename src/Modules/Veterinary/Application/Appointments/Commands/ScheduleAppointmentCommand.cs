using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;

namespace Veterinary.Application.Appointments.Commands;

[AuthorizeRequest(AuthorizationPolicies.Veterinarian)]
public record ScheduleAppointmentCommand(
    Guid TutorId,
    Guid PetId,
    Guid VeterinarianId,
    DateTimeOffset Date,
    int DurationInMinutes,
    string Reason) : ICommand<Guid>;
