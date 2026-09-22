using Core.Application.Messaging;
using Core.Domain;
using MediatR;
using TutorPortal.Application.Abstractions;
using TutorPortal.Application.Auth;
using TutorPortal.Application.PetHealth;
using TutorPortal.Application.Scheduling.Commands;

namespace TutorPortal.Application.Scheduling;

/// <summary>Books an appointment for an owned pet via the scheduling port.</summary>
public sealed class BookTutorAppointmentCommandHandler : IRequestHandler<BookTutorAppointmentCommand, Result<Guid>>
{
    private readonly TutorPortalUserResolver _userResolver;
    private readonly TutorPetAccessGuard _accessGuard;
    private readonly ITutorSchedulingPort _schedulingPort;

    /// <summary>Creates the handler.</summary>
    public BookTutorAppointmentCommandHandler(
        TutorPortalUserResolver userResolver,
        TutorPetAccessGuard accessGuard,
        ITutorSchedulingPort schedulingPort)
    {
        _userResolver = userResolver;
        _accessGuard = accessGuard;
        _schedulingPort = schedulingPort;
    }

    /// <inheritdoc />
    public async Task<Result<Guid>> Handle(BookTutorAppointmentCommand request, CancellationToken cancellationToken)
    {
        var tutorResult = await TutorBookingHandlerBase.EnsureTutorOwnsPetAsync(
            _userResolver, _accessGuard, request.PetId, cancellationToken);
        if (tutorResult.IsFailure)
        {
            return Result.Failure<Guid>(tutorResult.Error);
        }

        var appointmentId = request.Id == Guid.Empty ? Guid.NewGuid() : request.Id;
        return await _schedulingPort.BookAsync(
            appointmentId,
            tutorResult.Value,
            request.PetId,
            request.Kind,
            request.ServiceId,
            request.ProfessionalId,
            request.Date,
            cancellationToken);
    }
}

/// <summary>Cancels a tutor-owned appointment.</summary>
public sealed class CancelTutorAppointmentCommandHandler : IRequestHandler<CancelTutorAppointmentCommand, Result>
{
    private readonly TutorPortalUserResolver _userResolver;
    private readonly TutorPetAccessGuard _accessGuard;
    private readonly ITutorSchedulingPort _schedulingPort;

    /// <summary>Creates the handler.</summary>
    public CancelTutorAppointmentCommandHandler(
        TutorPortalUserResolver userResolver,
        TutorPetAccessGuard accessGuard,
        ITutorSchedulingPort schedulingPort)
    {
        _userResolver = userResolver;
        _accessGuard = accessGuard;
        _schedulingPort = schedulingPort;
    }

    /// <inheritdoc />
    public async Task<Result> Handle(CancelTutorAppointmentCommand request, CancellationToken cancellationToken)
    {
        var tutorResult = await TutorBookingHandlerBase.EnsureTutorOwnsPetAsync(
            _userResolver, _accessGuard, request.PetId, cancellationToken);
        if (tutorResult.IsFailure)
        {
            return Result.Failure(tutorResult.Error);
        }

        return await _schedulingPort.CancelAsync(
            tutorResult.Value,
            request.PetId,
            request.Kind,
            request.AppointmentId,
            cancellationToken);
    }
}
