using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Application.Messaging;
using Core.Domain.Authorization;

namespace Petshop.Application.GroomingSlots.Commands;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.GroomingWrite)]
public record DefineGroomingDailyAvailabilityCommand(
    Guid GroomerId,
    DateTimeOffset Date,
    TimeSpan DayStart,
    TimeSpan DayEnd,
    int SlotDurationMinutes,
    Guid IdempotencyKey = default) : IIdempotentCommand;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.GroomingWrite)]
public record BlockGroomingSlotCommand(Guid SlotId, Guid IdempotencyKey = default) : IIdempotentCommand;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.GroomingWrite)]
public record UnblockGroomingSlotCommand(Guid SlotId, Guid IdempotencyKey = default) : IIdempotentCommand;
