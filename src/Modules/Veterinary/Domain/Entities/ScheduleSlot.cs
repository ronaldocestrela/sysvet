using Core.Domain;

namespace Veterinary.Domain.Entities;

public class ScheduleSlot : Entity
{
    public Guid VeterinarianId { get; private set; }
    public DateTimeOffset Date { get; private set; }
    public TimeSpan StartTime { get; private set; }
    public TimeSpan EndTime { get; private set; }
    public bool IsAvailable { get; private set; }

    private ScheduleSlot() { } // EF Core

    public ScheduleSlot(Guid id, Guid veterinarianId, DateTimeOffset date, TimeSpan startTime, TimeSpan endTime)
    {
        Id = id;
        VeterinarianId = veterinarianId;
        Date = date;
        StartTime = startTime;
        EndTime = endTime;
        IsAvailable = true;
    }

    public void Book()
    {
        if (!IsAvailable)
            throw new InvalidOperationException("Slot is already booked.");
            
        IsAvailable = false;
    }

    public void CancelBooking()
    {
        IsAvailable = true;
    }
}
