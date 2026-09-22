using Core.Domain;
using TutorPortal.Application.Auth;
using TutorPortal.Application.PetHealth;

namespace TutorPortal.Application.Scheduling;

/// <summary>
/// Resolves tutor id and verifies pet ownership for scheduling handlers.
/// </summary>
internal static class TutorBookingHandlerBase
{
    /// <summary>
    /// Ensures the authenticated tutor owns the pet.
    /// </summary>
    internal static async Task<Result<Guid>> EnsureTutorOwnsPetAsync(
        TutorPortalUserResolver userResolver,
        TutorPetAccessGuard accessGuard,
        Guid petId,
        CancellationToken cancellationToken)
    {
        var tutorResult = await userResolver.ResolveTutorIdAsync(cancellationToken);
        if (tutorResult.IsFailure)
        {
            return Result.Failure<Guid>(tutorResult.Error);
        }

        var access = await accessGuard.EnsureOwnedAsync(petId, tutorResult.Value, cancellationToken);
        if (access.IsFailure)
        {
            return Result.Failure<Guid>(access.Error);
        }

        return Result.Success(tutorResult.Value);
    }
}
