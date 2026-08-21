using Core.Application.Messaging;
using Veterinary.Application.Appointments.DTOs;

namespace Veterinary.Application.Appointments.Queries;

public record GetDailyScheduleQuery(Guid VeterinarianId, DateTimeOffset Date) : IQuery<List<AppointmentDto>>;
