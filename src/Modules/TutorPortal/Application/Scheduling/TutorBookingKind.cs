namespace TutorPortal.Application.Scheduling;

/// <summary>
/// Discriminator for clinical vs grooming tutor bookings.
/// </summary>
public enum TutorBookingKind
{
    /// <summary>Veterinary consultation on the unified clinical agenda.</summary>
    Clinical = 0,

    /// <summary>Petshop banho e tosa agenda.</summary>
    Grooming = 1,
}
