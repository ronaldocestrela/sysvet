using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;

namespace Core.Application.AccessProfiles.Commands;

/// <summary>
/// Updates profile metadata and permission matrix.
/// </summary>
[AuthorizeRequest(AuthorizationPolicies.Admin)]
public sealed record UpdateAccessProfileCommand(
    Guid Id,
    string? Name,
    string? Description,
    decimal? MaxDiscountPercent,
    IReadOnlyList<string> PermissionCodes) : ICommand;
