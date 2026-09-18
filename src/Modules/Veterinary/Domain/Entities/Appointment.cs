using Core.Domain;
using Veterinary.Domain.Enums;
using Veterinary.Domain.Errors;

namespace Veterinary.Domain.Entities;

/// <summary>
/// Aggregate root for a clinical consultation booking on the unified veterinarian agenda.
/// </summary>
public sealed class Appointment : AggregateRoot
{
    public Guid TutorId { get; private set; }
    public Guid PetId { get; private set; }
    public Guid VeterinarianId { get; private set; }
    public DateTimeOffset Date { get; private set; }
    public int DurationInMinutes { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public AppointmentStatus Status { get; private set; }

    private Appointment(Guid id, Guid tutorId, Guid petId, Guid veterinarianId, DateTimeOffset date, int durationInMinutes, string reason, AppointmentStatus status)
    {
        Id = id;
        TutorId = tutorId;
        PetId = petId;
        VeterinarianId = veterinarianId;
        Date = date;
        DurationInMinutes = durationInMinutes;
        Reason = reason;
        Status = status;
    }

    private Appointment() { }

    /// <summary>Rehydrates an appointment from sync pull without scheduling validation.</summary>
    public static Appointment RestoreFromSync(
        Guid id,
        Guid tutorId,
        Guid petId,
        Guid veterinarianId,
        DateTimeOffset date,
        int durationInMinutes,
        string reason,
        AppointmentStatus status,
        DateTimeOffset updatedAt)
    {
        var appointment = new Appointment(id, tutorId, petId, veterinarianId, date, durationInMinutes, reason, status);
        appointment.UpdatedAt = updatedAt;
        return appointment;
    }

    /// <summary>Applies remote sync fields when remote wins LWW.</summary>
    public void ApplySyncSnapshot(
        DateTimeOffset date,
        int durationInMinutes,
        string reason,
        AppointmentStatus status,
        DateTimeOffset updatedAt)
    {
        Date = date;
        DurationInMinutes = durationInMinutes;
        Reason = reason;
        Status = status;
        UpdatedAt = updatedAt;
    }

    /// <summary>Creates a new appointment in <see cref="AppointmentStatus.Scheduled"/>.</summary>
    public static Result<Appointment> Create(
        Guid id,
        Guid tutorId,
        Guid petId,
        Guid veterinarianId,
        DateTimeOffset date,
        int durationInMinutes,
        string reason)
    {
        if (date < DateTimeOffset.UtcNow)
        {
            return Result.Failure<Appointment>(VeterinaryErrors.Appointment.InvalidDate);
        }

        var appointment = new Appointment(
            id,
            tutorId,
            petId,
            veterinarianId,
            date,
            durationInMinutes,
            reason,
            AppointmentStatus.Scheduled);

        appointment.Touch();
        return Result.Success(appointment);
    }

    /// <summary>Confirms attendance from scheduled state.</summary>
    public Result Confirm()
    {
        if (Status != AppointmentStatus.Scheduled)
        {
            return Result.Failure(VeterinaryErrors.Appointment.InvalidStatusTransition);
        }

        Status = AppointmentStatus.Confirmed;
        Touch();
        return Result.Success();
    }

    /// <summary>Marks the consultation as in progress.</summary>
    public Result Start()
    {
        if (Status != AppointmentStatus.Confirmed)
        {
            return Result.Failure(VeterinaryErrors.Appointment.InvalidStatusTransition);
        }

        Status = AppointmentStatus.InProgress;
        Touch();
        return Result.Success();
    }

    /// <summary>Completes an in-progress consultation.</summary>
    public Result Complete()
    {
        if (Status != AppointmentStatus.InProgress)
        {
            return Result.Failure(VeterinaryErrors.Appointment.InvalidStatusTransition);
        }

        Status = AppointmentStatus.Completed;
        Touch();
        return Result.Success();
    }

    /// <summary>Records patient no-show from scheduled or confirmed.</summary>
    public Result MarkNoShow()
    {
        if (Status is not (AppointmentStatus.Scheduled or AppointmentStatus.Confirmed))
        {
            return Result.Failure(VeterinaryErrors.Appointment.InvalidStatusTransition);
        }

        Status = AppointmentStatus.NoShow;
        Touch();
        return Result.Success();
    }

    /// <summary>Cancels the appointment unless already completed or cancelled.</summary>
    public Result Cancel()
    {
        if (Status is AppointmentStatus.Completed or AppointmentStatus.Cancelled or AppointmentStatus.NoShow)
        {
            return Result.Failure(VeterinaryErrors.Appointment.InvalidStatusTransition);
        }

        Status = AppointmentStatus.Cancelled;
        Touch();
        return Result.Success();
    }

    /// <summary>Moves the appointment to a new date/time and resets to scheduled.</summary>
    public Result Reschedule(DateTimeOffset newDate)
    {
        if (newDate < DateTimeOffset.UtcNow)
        {
            return Result.Failure(VeterinaryErrors.Appointment.InvalidDate);
        }

        if (Status is AppointmentStatus.Completed or AppointmentStatus.Cancelled or AppointmentStatus.NoShow)
        {
            return Result.Failure(VeterinaryErrors.Appointment.InvalidStatusTransition);
        }

        Date = newDate;
        Status = AppointmentStatus.Scheduled;
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
