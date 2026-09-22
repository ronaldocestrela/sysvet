using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;
using TutorPortal.Application.PetHealth.Dtos;

namespace TutorPortal.Application.PetHealth.Queries;

/// <summary>Returns the vaccination card for a tutor-owned pet.</summary>
[AuthorizeRequest(AuthorizationPolicies.TutorPortal)]
public sealed record GetTutorVaccinationCardQuery(Guid PetId) : IQuery<TutorVaccinationCardDto>;

/// <summary>Lists clinical exams for a tutor-owned pet.</summary>
[AuthorizeRequest(AuthorizationPolicies.TutorPortal)]
public sealed record ListTutorPetExamsQuery(Guid PetId) : IQuery<IReadOnlyList<TutorPetExamDto>>;

/// <summary>Lists sanitized visit timeline for a tutor-owned pet.</summary>
[AuthorizeRequest(AuthorizationPolicies.TutorPortal)]
public sealed record ListTutorPetTimelineQuery(Guid PetId) : IQuery<IReadOnlyList<TutorPetTimelineItemDto>>;
