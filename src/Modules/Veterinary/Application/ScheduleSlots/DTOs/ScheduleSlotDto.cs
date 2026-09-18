namespace Veterinary.Application.ScheduleSlots.DTOs;

public class ScheduleSlotDto
{
    public Guid Id { get; set; }
    public Guid VeterinarianId { get; set; }
    public DateTimeOffset Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsAvailable { get; set; }
}
