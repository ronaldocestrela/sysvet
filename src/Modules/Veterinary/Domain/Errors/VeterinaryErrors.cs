using Core.Domain;

namespace Veterinary.Domain.Errors;

public static class VeterinaryErrors
{
    public static class Appointment
    {
        public static readonly Error InvalidDate = new(
            "Appointment.InvalidDate", 
            "The appointment date cannot be in the past.");
            
        public static readonly Error InvalidStatusTransition = new(
            "Appointment.InvalidStatusTransition", 
            "The appointment cannot transition to this status.");
    }
}
