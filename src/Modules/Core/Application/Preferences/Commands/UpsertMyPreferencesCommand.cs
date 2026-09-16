using Core.Application.Authorization;
using Core.Application.Behaviors;
using Core.Application.Messaging;

namespace Core.Application.Preferences.Commands;

/// <summary>
/// Updates UI preferences for the authenticated user.
/// </summary>
[AuthorizeRequest(AuthorizationPolicies.Authenticated)]
public sealed record UpsertMyPreferencesCommand(
    string ShortcutsJson,
    string SavedFiltersJson,
    string ColumnLayoutsJson) : ICommand;
