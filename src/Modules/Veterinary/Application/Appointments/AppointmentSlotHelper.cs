using Veterinary.Domain.Entities;

namespace Veterinary.Application.Appointments;

internal static class AppointmentSlotHelper
{
    internal static ScheduleSlot? FindCoveringSlot(IEnumerable<ScheduleSlot> slots, DateTimeOffset date, int durationMinutes)
    {
        var startTime = date.TimeOfDay;
        var endTime = startTime.Add(TimeSpan.FromMinutes(durationMinutes));
        return slots.FirstOrDefault(s => s.StartTime <= startTime && s.EndTime >= endTime);
    }
}
