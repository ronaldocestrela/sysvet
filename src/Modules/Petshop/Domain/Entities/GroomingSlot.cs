using Core.Domain;
using Petshop.Domain;

namespace Petshop.Domain.Entities;

/// <summary>
/// A bookable time window on a groomer's daily agenda.
/// </summary>
public class GroomingSlot : Entity
{
    /// <summary>Groomer staff user id.</summary>
    public Guid GroomerId { get; private set; }

    /// <summary>Calendar day for the slot.</summary>
    public DateTimeOffset Date { get; private set; }

    /// <summary>Start time on <see cref="Date"/>.</summary>
    public TimeSpan StartTime { get; private set; }

    /// <summary>End time on <see cref="Date"/>.</summary>
    public TimeSpan EndTime { get; private set; }

    /// <summary>When false, the slot is booked or blocked.</summary>
    public bool IsAvailable { get; private set; }

    private GroomingSlot() { }

    /// <summary>Creates an available slot for scheduling.</summary>
    public GroomingSlot(Guid id, Guid groomerId, DateTimeOffset date, TimeSpan startTime, TimeSpan endTime)
    {
        Id = id;
        GroomerId = groomerId;
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
            return Result.Failure(ErrorCodes.GroomingSlot.NotAvailable);
        }

        IsAvailable = false;
        Touch();
        return Result.Success();
    }

    /// <summary>Administratively blocks the slot.</summary>
    public Result Block()
    {
        if (!IsAvailable)
        {
            return Result.Failure(ErrorCodes.GroomingSlot.NotAvailable);
        }

        IsAvailable = false;
        Touch();
        return Result.Success();
    }

    /// <summary>Reopens a blocked slot.</summary>
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
