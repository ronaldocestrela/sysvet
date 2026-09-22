using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;

namespace TutorPortal.Application.Push.Commands;

/// <summary>Registers or refreshes a Web Push subscription for the authenticated tutor.</summary>
[AuthorizeRequest(AuthorizationPolicies.TutorPortal)]
public sealed record SubscribeTutorPushCommand(string Endpoint, string P256dh, string Auth, string? UserAgent) : ICommand;

/// <summary>Removes a Web Push subscription for the authenticated tutor.</summary>
[AuthorizeRequest(AuthorizationPolicies.TutorPortal)]
public sealed record UnsubscribeTutorPushCommand(string Endpoint) : ICommand;
