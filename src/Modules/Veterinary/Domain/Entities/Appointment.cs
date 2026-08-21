using Core.Domain;

namespace Veterinary.Domain.Entities;

public class Appointment : Entity
{
    public Guid TutorId { get; private set; }
    public Guid PetId { get; private set; }
    public Guid VeterinarianId { get; private set; }
    
    public DateTimeOffset Date { get; private set; }
    public int DurationInMinutes { get; private set; }
    public AppointmentStatus Status { get; private set; }
    public string? Notes { get; private set; }

    private Appointment() { } // EF Core

    public Appointment(Guid id, Guid tutorId, Guid petId, Guid veterinarianId, DateTimeOffset date, int durationInMinutes, string? notes)
    {
        Id = id;
        TutorId = tutorId;
        PetId = petId;
        VeterinarianId = veterinarianId;
        Date = date;
        DurationInMinutes = durationInMinutes;
        Status = AppointmentStatus.Agendado;
        Notes = notes;
    }

    public void UpdateStatus(AppointmentStatus newStatus)
    {
        Status = newStatus;
    }
    
    public void UpdateNotes(string? notes)
    {
        Notes = notes;
    }
}
