using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;
using Veterinary.Application.Appointments.DTOs;

namespace Veterinary.Application.Appointments.Queries;

[AuthorizeRequest(AuthorizationPolicies.Veterinarian)]
public record GetDailyScheduleQuery(Guid VeterinarianId, DateTimeOffset Date) : IQuery<List<AppointmentDto>>;
