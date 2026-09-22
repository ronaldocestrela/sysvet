namespace TutorPortal.Application.Auth.Dtos;

/// <summary>
/// Profile returned by <c>GET /api/v1/tutor-portal/me</c>.
/// </summary>
public sealed record TutorPortalMeDto(
    string UserId,
    string Email,
    Guid TenantId,
    Guid TutorId,
    string Name,
    IReadOnlyList<TutorPortalPetDto> Pets);

/// <summary>
/// Pet summary visible to the authenticated tutor.
/// </summary>
public sealed record TutorPortalPetDto(Guid Id, string Name);
