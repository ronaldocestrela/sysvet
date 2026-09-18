using Core.Domain;
using Veterinary.Domain;

namespace Veterinary.Domain.Entities;

/// <summary>
/// A bookable time window on a veterinarian's daily agenda; can be blocked without an appointment.
/// </summary>
public class ScheduleSlot : Entity
{
    /// <summary>Veterinarian who owns this slot.</summary>
    public Guid VeterinarianId { get; private set; }

    /// <summary>Calendar day (date component) for the slot.</summary>
    public DateTimeOffset Date { get; private set; }

    /// <summary>Start time on <see cref="Date"/>.</summary>
    public TimeSpan StartTime { get; private set; }

    /// <summary>End time on <see cref="Date"/>.</summary>
    public TimeSpan EndTime { get; private set; }

    /// <summary>When false, the slot is booked or administratively blocked.</summary>
    public bool IsAvailable { get; private set; }

    private ScheduleSlot() { }

    /// <summary>Creates an available slot for scheduling.</summary>
    public ScheduleSlot(Guid id, Guid veterinarianId, DateTimeOffset date, TimeSpan startTime, TimeSpan endTime)
    {
        Id = id;
        VeterinarianId = veterinarianId;
        Date = date;
        StartTime = startTime;
        EndTime = endTime;
        IsAvailable = true;
    }

    /// <summary>Reserves the slot for an appointment.</summary>
    public Result Book()
    {
        if (!IsAvailable)
        {
            return Result.Failure(ErrorCodes.ScheduleSlot.NotAvailable);
        }

        IsAvailable = false;
        Touch();
        return Result.Success();
    }

    /// <summary>Administratively blocks the slot without creating an appointment.</summary>
    public Result Block()
    {
        if (!IsAvailable)
        {
            return Result.Failure(ErrorCodes.ScheduleSlot.NotAvailable);
        }

        IsAvailable = false;
        Touch();
        return Result.Success();
    }

    /// <summary>Reopens a blocked slot that is not tied to an active booking workflow.</summary>
    public Result Unblock()
    {
        IsAvailable = true;
        Touch();
        return Result.Success();
    }

    /// <summary>Releases the slot after cancellation or reschedule.</summary>
    public Result CancelBooking()
    {
        IsAvailable = true;
        Touch();
        return Result.Success();
    }

    private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
}
