using TutorPortal.Application.PetHealth.Dtos;

namespace TutorPortal.Application.Abstractions;

/// <summary>
/// Cross-module read model for tutor-visible pet health data (vaccines, exams, visits).
/// </summary>
public interface ITutorPetHealthReadPort
{
    /// <summary>
    /// Builds the digital vaccination card for an authorized pet.
    /// </summary>
    Task<TutorVaccinationCardDto?> GetVaccinationCardAsync(Guid petId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists clinical exams for a pet ordered by most recent activity.
    /// </summary>
    Task<IReadOnlyList<TutorPetExamDto>> ListExamsAsync(Guid petId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists sanitized visit timeline entries (clinical + grooming) for a pet.
    /// </summary>
    Task<IReadOnlyList<TutorPetTimelineItemDto>> ListTimelineAsync(Guid petId, CancellationToken cancellationToken = default);
}
