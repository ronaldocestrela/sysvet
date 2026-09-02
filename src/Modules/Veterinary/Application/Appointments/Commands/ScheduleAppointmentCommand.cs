using Core.Application.Messaging;

namespace Veterinary.Application.Appointments.Commands;

public record ScheduleAppointmentCommand(
    Guid TutorId,
    Guid PetId,
    Guid VeterinarianId,
    DateTimeOffset Date,
    int DurationInMinutes,
    string Reason) : ICommand<Guid>;
