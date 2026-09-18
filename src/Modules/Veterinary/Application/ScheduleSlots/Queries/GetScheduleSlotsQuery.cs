using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;
using Core.Domain.Authorization;
using Veterinary.Application.ScheduleSlots.DTOs;

namespace Veterinary.Application.ScheduleSlots.Queries;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.AppointmentsRead)]
public record GetScheduleSlotsQuery(Guid VeterinarianId, DateTimeOffset Date) : IQuery<List<ScheduleSlotDto>>;
