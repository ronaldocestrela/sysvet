using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Application.Messaging;
using Core.Domain.Authorization;

namespace Petshop.Application.GroomingSlots.Queries;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.GroomingRead)]
public record GetGroomingScheduleSlotsQuery(Guid GroomerId, DateTimeOffset Date) : IQuery<List<GroomingSlotDto>>;

/// <summary>Schedule slot row for API.</summary>
public sealed record GroomingSlotDto(Guid Id, Guid GroomerId, DateTimeOffset Date, TimeSpan StartTime, TimeSpan EndTime, bool IsAvailable);
