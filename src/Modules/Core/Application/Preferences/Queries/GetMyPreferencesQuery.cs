using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;
using Core.Application.Preferences.Dtos;

namespace Core.Application.Preferences.Queries;

/// <summary>
/// Loads UI preferences for the authenticated user.
/// </summary>
[AuthorizeRequest(AuthorizationPolicies.Authenticated)]
public sealed record GetMyPreferencesQuery : IQuery<UserPreferenceDto>;
