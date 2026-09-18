using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Application.Messaging;
using Core.Domain.Authorization;

namespace Veterinary.Application.ScheduleSlots.Commands;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.AppointmentsWrite)]
public record DefineDailyAvailabilityCommand(
    Guid VeterinarianId,
    DateTimeOffset Date,
    TimeSpan DayStart,
    TimeSpan DayEnd,
    int SlotDurationMinutes,
    Guid IdempotencyKey = default) : IIdempotentCommand;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.AppointmentsWrite)]
public record BlockScheduleSlotCommand(Guid ScheduleSlotId, Guid IdempotencyKey = default) : IIdempotentCommand;

[AuthorizeRequest(AuthorizationPolicies.ClinicStaff, Permissions.AppointmentsWrite)]
public record UnblockScheduleSlotCommand(Guid ScheduleSlotId, Guid IdempotencyKey = default) : IIdempotentCommand;
