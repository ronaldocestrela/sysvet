using System;
using Core.Domain;
using Veterinary.Domain.Enums;
using Veterinary.Domain.Errors;

namespace Veterinary.Domain.Entities;

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

    private Appointment() { } // EF Core

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

        return Result.Success(appointment);
    }

    public Result Confirm()
    {
        if (Status != AppointmentStatus.Scheduled)
        {
            return Result.Failure(VeterinaryErrors.Appointment.InvalidStatusTransition);
        }

        Status = AppointmentStatus.Confirmed;
        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status == AppointmentStatus.Completed || Status == AppointmentStatus.Cancelled)
        {
            return Result.Failure(VeterinaryErrors.Appointment.InvalidStatusTransition);
        }

        Status = AppointmentStatus.Cancelled;
        return Result.Success();
    }

    public Result Reschedule(DateTimeOffset newDate)
    {
        if (newDate < DateTimeOffset.UtcNow)
        {
            return Result.Failure(VeterinaryErrors.Appointment.InvalidDate);
        }

        if (Status == AppointmentStatus.Completed || Status == AppointmentStatus.Cancelled)
        {
            return Result.Failure(VeterinaryErrors.Appointment.InvalidStatusTransition);
        }

        Date = newDate;
        Status = AppointmentStatus.Scheduled;

        return Result.Success();
    }
}
