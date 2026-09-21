using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Application.Messaging;
using Core.Domain.Authorization;
using Petshop.Application.GroomingAppointments.DTOs;

namespace Petshop.Application.GroomingAppointments.Queries;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.GroomingRead)]
public record GetDailyGroomingScheduleQuery(Guid? GroomerId, DateTimeOffset Date) : IQuery<List<GroomingAppointmentDto>>;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.GroomingRead)]
public record GetGroomingAppointmentByIdQuery(Guid Id) : IQuery<GroomingAppointmentDto>;
