namespace Veterinary.Application.Appointments.DTOs;

public class AppointmentDto
{
    public Guid Id { get; set; }
    public Guid TutorId { get; set; }
    public Guid PetId { get; set; }
    public Guid VeterinarianId { get; set; }
    public DateTimeOffset Date { get; set; }
    public int DurationInMinutes { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
