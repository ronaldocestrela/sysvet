using Core.Domain;
using Petshop.Domain.Enums;
using Petshop.Domain.Events;

using ErrorCodes = Petshop.Domain.ErrorCodes;

namespace Petshop.Domain.Entities;

/// <summary>
/// Aggregate root for a grooming salon booking.
/// </summary>
public sealed class GroomingAppointment : AggregateRoot
{
    public Guid TutorId { get; private set; }
    public Guid PetId { get; private set; }
    public Guid GroomerId { get; private set; }
    public Guid GroomingServiceId { get; private set; }
    public DateTimeOffset Date { get; private set; }
    public int DurationInMinutes { get; private set; }
    public string Notes { get; private set; } = string.Empty;
    public GroomingAppointmentStatus Status { get; private set; }

    private GroomingAppointment(
        Guid id,
        Guid tutorId,
        Guid petId,
        Guid groomerId,
        Guid groomingServiceId,
        DateTimeOffset date,
        int durationInMinutes,
        string notes,
        GroomingAppointmentStatus status)
        : base(id)
    {
        TutorId = tutorId;
        PetId = petId;
        GroomerId = groomerId;
        GroomingServiceId = groomingServiceId;
        Date = date;
        DurationInMinutes = durationInMinutes;
        Notes = notes;
        Status = status;
    }

    private GroomingAppointment() { }

    /// <summary>Rehydrates from sync pull without scheduling validation.</summary>
    public static GroomingAppointment RestoreFromSync(
        Guid id,
        Guid tutorId,
        Guid petId,
        Guid groomerId,
        Guid groomingServiceId,
        DateTimeOffset date,
        int durationInMinutes,
        string notes,
        GroomingAppointmentStatus status,
        DateTimeOffset updatedAt)
    {
        var appointment = new GroomingAppointment(id, tutorId, petId, groomerId, groomingServiceId, date, durationInMinutes, notes, status);
        appointment.UpdatedAt = updatedAt;
        return appointment;
    }

    /// <summary>Applies remote sync snapshot when remote wins LWW.</summary>
    public void ApplySyncSnapshot(
        DateTimeOffset date,
        int durationInMinutes,
        string notes,
        GroomingAppointmentStatus status,
        DateTimeOffset updatedAt)
    {
        Date = date;
        DurationInMinutes = durationInMinutes;
        Notes = notes;
        Status = status;
        UpdatedAt = updatedAt;
    }

    /// <summary>Creates a new appointment in scheduled state.</summary>
    public static Result<GroomingAppointment> Create(
        Guid id,
        Guid tutorId,
        Guid petId,
        Guid groomerId,
        Guid groomingServiceId,
        DateTimeOffset date,
        int durationInMinutes,
        string notes)
    {
        if (date < DateTimeOffset.UtcNow)
        {
            return Result.Failure<GroomingAppointment>(ErrorCodes.GroomingAppointment.InvalidDate);
        }

        var appointment = new GroomingAppointment(
            id,
            tutorId,
            petId,
            groomerId,
            groomingServiceId,
            date,
            durationInMinutes,
            notes ?? string.Empty,
            GroomingAppointmentStatus.Scheduled);

        appointment.Touch();
        return Result.Success(appointment);
    }

    /// <summary>Confirms attendance from scheduled state.</summary>
    public Result Confirm()
    {
        if (Status != GroomingAppointmentStatus.Scheduled)
        {
            return Result.Failure(ErrorCodes.GroomingAppointment.InvalidStatusTransition);
        }

        Status = GroomingAppointmentStatus.Confirmed;
        Touch();
        return Result.Success();
    }

    /// <summary>Marks the service as in progress.</summary>
    public Result Start()
    {
        if (Status != GroomingAppointmentStatus.Confirmed)
        {
            return Result.Failure(ErrorCodes.GroomingAppointment.InvalidStatusTransition);
        }

        Status = GroomingAppointmentStatus.InProgress;
        Raise(new GroomingStartedDomainEvent(Id, PetId, TutorId, DateTimeOffset.UtcNow));
        Touch();
        return Result.Success();
    }

    /// <summary>Marks the pet ready for tutor pickup while service is in progress.</summary>
    public Result MarkReady()
    {
        if (Status == GroomingAppointmentStatus.ReadyForPickup)
        {
            return Result.Success();
        }

        if (Status != GroomingAppointmentStatus.InProgress)
        {
            return Result.Failure(ErrorCodes.GroomingAppointment.InvalidStatusTransition);
        }

        Status = GroomingAppointmentStatus.ReadyForPickup;
        Raise(new GroomingReadyForPickupDomainEvent(Id, PetId, TutorId, DateTimeOffset.UtcNow));
        Touch();
        return Result.Success();
    }

    /// <summary>Completes grooming from in-progress (implicit ready) or ready-for-pickup.</summary>
    public Result Complete()
    {
        if (Status == GroomingAppointmentStatus.InProgress)
        {
            var ready = MarkReady();
            if (ready.IsFailure)
            {
                return ready;
            }
        }
        else if (Status != GroomingAppointmentStatus.ReadyForPickup)
        {
            return Result.Failure(ErrorCodes.GroomingAppointment.InvalidStatusTransition);
        }

        Status = GroomingAppointmentStatus.Completed;
        Raise(new GroomingCompletedDomainEvent(Id, PetId, TutorId, DateTimeOffset.UtcNow));
        Touch();
        return Result.Success();
    }

    /// <summary>Records patient no-show.</summary>
    public Result MarkNoShow()
    {
        if (Status is not (GroomingAppointmentStatus.Scheduled or GroomingAppointmentStatus.Confirmed))
        {
            return Result.Failure(ErrorCodes.GroomingAppointment.InvalidStatusTransition);
        }

        Status = GroomingAppointmentStatus.NoShow;
        Touch();
        return Result.Success();
    }

    /// <summary>Cancels unless already terminal.</summary>
    public Result Cancel()
    {
        if (Status is GroomingAppointmentStatus.Completed or GroomingAppointmentStatus.Cancelled or GroomingAppointmentStatus.NoShow)
        {
            return Result.Failure(ErrorCodes.GroomingAppointment.InvalidStatusTransition);
        }

        Status = GroomingAppointmentStatus.Cancelled;
        Touch();
        return Result.Success();
    }

    /// <summary>Moves to a new date/time and resets to scheduled.</summary>
    public Result Reschedule(DateTimeOffset newDate)
    {
        if (newDate < DateTimeOffset.UtcNow)
        {
            return Result.Failure(ErrorCodes.GroomingAppointment.InvalidDate);
        }

        if (Status is GroomingAppointmentStatus.Completed or GroomingAppointmentStatus.Cancelled or GroomingAppointmentStatus.NoShow)
        {
            return Result.Failure(ErrorCodes.GroomingAppointment.InvalidStatusTransition);
        }

        Date = newDate;
        Status = GroomingAppointmentStatus.Scheduled;
        Touch();
        return Result.Success();
    }

    /// <summary>Whether this appointment occupies the agenda at the given instant.</summary>
    public bool Overlaps(DateTimeOffset start, DateTimeOffset end)
    {
        var appointmentEnd = Date.AddMinutes(DurationInMinutes);
        return Date < end && appointmentEnd > start;
    }

    private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
}
