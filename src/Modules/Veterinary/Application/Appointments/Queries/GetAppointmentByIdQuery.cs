using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;
using Core.Domain.Authorization;
using Veterinary.Application.Appointments.DTOs;

namespace Veterinary.Application.Appointments.Queries;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.AppointmentsRead)]
public record GetAppointmentByIdQuery(Guid Id) : IQuery<AppointmentDto>;
