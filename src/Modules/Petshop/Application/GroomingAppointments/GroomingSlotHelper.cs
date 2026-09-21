using Petshop.Domain.Entities;

namespace Petshop.Application.GroomingAppointments;

internal static class GroomingSlotHelper
{
    internal static GroomingSlot? FindCoveringSlot(IEnumerable<GroomingSlot> slots, DateTimeOffset date, int durationMinutes)
    {
        var startTime = date.TimeOfDay;
        var endTime = startTime.Add(TimeSpan.FromMinutes(durationMinutes));
        return slots.FirstOrDefault(s => s.StartTime <= startTime && s.EndTime >= endTime);
    }
}
