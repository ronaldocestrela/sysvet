using Core.Application.Messaging;
using Core.Domain;
using MediatR;
using TutorPortal.Application.Abstractions;
using TutorPortal.Application.Auth;
using TutorPortal.Application.PetHealth;
using TutorPortal.Application.Scheduling.Dtos;
using TutorPortal.Application.Scheduling.Queries;

namespace TutorPortal.Application.Scheduling;

/// <summary>Loads bookable services after pet ownership check.</summary>
public sealed class ListTutorBookableServicesQueryHandler
    : IRequestHandler<ListTutorBookableServicesQuery, Result<IReadOnlyList<TutorBookableServiceDto>>>
{
    private readonly TutorPortalUserResolver _userResolver;
    private readonly TutorPetAccessGuard _accessGuard;
    private readonly ITutorSchedulingPort _schedulingPort;

    /// <summary>Creates the handler.</summary>
    public ListTutorBookableServicesQueryHandler(
        TutorPortalUserResolver userResolver,
        TutorPetAccessGuard accessGuard,
        ITutorSchedulingPort schedulingPort)
    {
        _userResolver = userResolver;
        _accessGuard = accessGuard;
        _schedulingPort = schedulingPort;
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<TutorBookableServiceDto>>> Handle(
        ListTutorBookableServicesQuery request,
        CancellationToken cancellationToken)
    {
        var tutorResult = await TutorBookingHandlerBase.EnsureTutorOwnsPetAsync(
            _userResolver, _accessGuard, request.PetId, cancellationToken);
        if (tutorResult.IsFailure)
        {
            return Result.Failure<IReadOnlyList<TutorBookableServiceDto>>(tutorResult.Error);
        }

        var services = await _schedulingPort.ListServicesAsync(cancellationToken);
        return Result.Success(services);
    }
}

/// <summary>Lists professionals for a service/day after pet ownership check.</summary>
public sealed class ListTutorBookingProfessionalsQueryHandler
    : IRequestHandler<ListTutorBookingProfessionalsQuery, Result<IReadOnlyList<TutorBookingProfessionalDto>>>
{
    private readonly TutorPortalUserResolver _userResolver;
    private readonly TutorPetAccessGuard _accessGuard;
    private readonly ITutorSchedulingPort _schedulingPort;

    /// <summary>Creates the handler.</summary>
    public ListTutorBookingProfessionalsQueryHandler(
        TutorPortalUserResolver userResolver,
        TutorPetAccessGuard accessGuard,
        ITutorSchedulingPort schedulingPort)
    {
        _userResolver = userResolver;
        _accessGuard = accessGuard;
        _schedulingPort = schedulingPort;
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<TutorBookingProfessionalDto>>> Handle(
        ListTutorBookingProfessionalsQuery request,
        CancellationToken cancellationToken)
    {
        var tutorResult = await TutorBookingHandlerBase.EnsureTutorOwnsPetAsync(
            _userResolver, _accessGuard, request.PetId, cancellationToken);
        if (tutorResult.IsFailure)
        {
            return Result.Failure<IReadOnlyList<TutorBookingProfessionalDto>>(tutorResult.Error);
        }

        var professionals = await _schedulingPort.ListProfessionalsAsync(
            request.Kind, request.ServiceId, request.Date, cancellationToken);
        return Result.Success(professionals);
    }
}

/// <summary>Lists available slots after pet ownership check.</summary>
public sealed class ListTutorAvailableSlotsQueryHandler
    : IRequestHandler<ListTutorAvailableSlotsQuery, Result<IReadOnlyList<TutorAvailableSlotDto>>>
{
    private readonly TutorPortalUserResolver _userResolver;
    private readonly TutorPetAccessGuard _accessGuard;
    private readonly ITutorSchedulingPort _schedulingPort;

    /// <summary>Creates the handler.</summary>
    public ListTutorAvailableSlotsQueryHandler(
        TutorPortalUserResolver userResolver,
        TutorPetAccessGuard accessGuard,
        ITutorSchedulingPort schedulingPort)
    {
        _userResolver = userResolver;
        _accessGuard = accessGuard;
        _schedulingPort = schedulingPort;
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<TutorAvailableSlotDto>>> Handle(
        ListTutorAvailableSlotsQuery request,
        CancellationToken cancellationToken)
    {
        var tutorResult = await TutorBookingHandlerBase.EnsureTutorOwnsPetAsync(
            _userResolver, _accessGuard, request.PetId, cancellationToken);
        if (tutorResult.IsFailure)
        {
            return Result.Failure<IReadOnlyList<TutorAvailableSlotDto>>(tutorResult.Error);
        }

        var slots = await _schedulingPort.ListAvailableSlotsAsync(
            request.Kind, request.ServiceId, request.ProfessionalId, request.Date, cancellationToken);
        return Result.Success(slots);
    }
}

/// <summary>Lists pet appointments after ownership check.</summary>
public sealed class ListTutorPetAppointmentsQueryHandler
    : IRequestHandler<ListTutorPetAppointmentsQuery, Result<IReadOnlyList<TutorPetAppointmentDto>>>
{
    private readonly TutorPortalUserResolver _userResolver;
    private readonly TutorPetAccessGuard _accessGuard;
    private readonly ITutorSchedulingPort _schedulingPort;

    /// <summary>Creates the handler.</summary>
    public ListTutorPetAppointmentsQueryHandler(
        TutorPortalUserResolver userResolver,
        TutorPetAccessGuard accessGuard,
        ITutorSchedulingPort schedulingPort)
    {
        _userResolver = userResolver;
        _accessGuard = accessGuard;
        _schedulingPort = schedulingPort;
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<TutorPetAppointmentDto>>> Handle(
        ListTutorPetAppointmentsQuery request,
        CancellationToken cancellationToken)
    {
        var tutorResult = await TutorBookingHandlerBase.EnsureTutorOwnsPetAsync(
            _userResolver, _accessGuard, request.PetId, cancellationToken);
        if (tutorResult.IsFailure)
        {
            return Result.Failure<IReadOnlyList<TutorPetAppointmentDto>>(tutorResult.Error);
        }

        var appointments = await _schedulingPort.ListAppointmentsAsync(tutorResult.Value, request.PetId, cancellationToken);
        return Result.Success(appointments);
    }
}
