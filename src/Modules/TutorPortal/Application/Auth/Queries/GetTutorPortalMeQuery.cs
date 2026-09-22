using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;
using TutorPortal.Application.Auth.Dtos;

namespace TutorPortal.Application.Auth.Queries;

/// <summary>
/// Returns the authenticated tutor profile and linked pets.
/// </summary>
[AuthorizeRequest(AuthorizationPolicies.TutorPortal)]
public sealed record GetTutorPortalMeQuery : IQuery<TutorPortalMeDto>;
