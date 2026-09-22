using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Common;
using Core.Application.Messaging;
using TutorPortal.Application.Scheduling;

namespace TutorPortal.Application.Scheduling.Commands;

/// <summary>Books a clinical or grooming appointment for an owned pet.</summary>
[AuthorizeRequest(AuthorizationPolicies.TutorPortal)]
public sealed record BookTutorAppointmentCommand(
    Guid PetId,
    TutorBookingKind Kind,
    Guid ServiceId,
    Guid ProfessionalId,
    DateTimeOffset Date,
    Guid Id,
    Guid IdempotencyKey = default) : IIdempotentCommand<Guid>;

/// <summary>Cancels a tutor-owned appointment.</summary>
[AuthorizeRequest(AuthorizationPolicies.TutorPortal)]
public sealed record CancelTutorAppointmentCommand(
    Guid PetId,
    TutorBookingKind Kind,
    Guid AppointmentId) : ICommand;
