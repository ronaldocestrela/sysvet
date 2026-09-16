namespace Core.Application.Preferences.Dtos;

/// <summary>
/// UI preferences for the authenticated user.
/// </summary>
public sealed record UserPreferenceDto(
    string ShortcutsJson,
    string SavedFiltersJson,
    string ColumnLayoutsJson);
