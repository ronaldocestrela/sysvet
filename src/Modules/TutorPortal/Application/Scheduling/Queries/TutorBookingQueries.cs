using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;
using TutorPortal.Application.Scheduling.Dtos;

namespace TutorPortal.Application.Scheduling.Queries;

/// <summary>Lists bookable services for tutor self-scheduling.</summary>
[AuthorizeRequest(AuthorizationPolicies.TutorPortal)]
public sealed record ListTutorBookableServicesQuery(Guid PetId) : IQuery<IReadOnlyList<TutorBookableServiceDto>>;

/// <summary>Lists professionals with availability on a day.</summary>
[AuthorizeRequest(AuthorizationPolicies.TutorPortal)]
public sealed record ListTutorBookingProfessionalsQuery(
    Guid PetId,
    TutorBookingKind Kind,
    Guid ServiceId,
    DateTimeOffset Date) : IQuery<IReadOnlyList<TutorBookingProfessionalDto>>;

/// <summary>Lists available start times for booking.</summary>
[AuthorizeRequest(AuthorizationPolicies.TutorPortal)]
public sealed record ListTutorAvailableSlotsQuery(
    Guid PetId,
    TutorBookingKind Kind,
    Guid ServiceId,
    Guid ProfessionalId,
    DateTimeOffset Date) : IQuery<IReadOnlyList<TutorAvailableSlotDto>>;

/// <summary>Lists tutor-owned appointments for a pet.</summary>
[AuthorizeRequest(AuthorizationPolicies.TutorPortal)]
public sealed record ListTutorPetAppointmentsQuery(Guid PetId) : IQuery<IReadOnlyList<TutorPetAppointmentDto>>;
