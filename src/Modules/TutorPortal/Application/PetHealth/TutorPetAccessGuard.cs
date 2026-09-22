using Core.Domain;
using Core.Domain.Entities;

namespace TutorPortal.Application.PetHealth;

/// <summary>
/// Ensures a pet belongs to the authenticated tutor before exposing health data.
/// </summary>
public sealed class TutorPetAccessGuard
{
    private readonly IPetRepository _petRepository;

    /// <summary>
    /// Creates the guard with CRM pet repository access.
    /// </summary>
    public TutorPetAccessGuard(IPetRepository petRepository)
    {
        _petRepository = petRepository;
    }

    /// <summary>
    /// Loads the pet when it is active and owned by the tutor; otherwise returns <see cref="TutorPortal.Domain.ErrorCodes.Pet.NotFound"/>.
    /// </summary>
    public async Task<Result<Pet>> EnsureOwnedAsync(Guid petId, Guid tutorId, CancellationToken cancellationToken)
    {
        var pet = await _petRepository.GetByIdAsync(petId, cancellationToken);
        if (pet is null || pet.IsDeleted || pet.TutorId != tutorId)
        {
            return Result.Failure<Pet>(TutorPortal.Domain.ErrorCodes.Pet.NotFound);
        }

        return Result.Success(pet);
    }
}
