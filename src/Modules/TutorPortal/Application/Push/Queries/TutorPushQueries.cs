using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;

namespace TutorPortal.Application.Push.Queries;

/// <summary>Returns the VAPID public key when push is configured on the server.</summary>
[AuthorizeRequest(AuthorizationPolicies.TutorPortal)]
public sealed record GetTutorPushVapidPublicKeyQuery : IQuery<string?>;
