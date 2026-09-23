using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;
using Core.Domain.Authorization;

namespace Core.Application.Privacy.Commands;

/// <summary>Anonymizes tutor personal identifiers and propagates tombstones to modules.</summary>
[AuthorizeRequest(AuthorizationPolicies.Admin, Permissions.PrivacyErase)]
public sealed record AnonymizeTutorCommand(Guid TutorId) : ICommand;
