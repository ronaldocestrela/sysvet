namespace Core.Domain.Entities;

/// <summary>
/// Per-user UI preferences stored as versioned JSON blobs on the server.
/// </summary>
public sealed class UserPreference : Entity
{
    /// <summary>
    /// Maximum length for each JSON payload field.
    /// </summary>
    public const int MaxJsonLength = 8192;

    /// <summary>
    /// ASP.NET Identity user identifier (string subject).
    /// </summary>
    public string UserId { get; private set; } = string.Empty;

    /// <summary>
    /// Keyboard shortcuts configuration JSON.
    /// </summary>
    public string ShortcutsJson { get; private set; } = "{}";

    /// <summary>
    /// Saved list filters JSON.
    /// </summary>
    public string SavedFiltersJson { get; private set; } = "{}";

    /// <summary>
    /// Column layout preferences JSON.
    /// </summary>
    public string ColumnLayoutsJson { get; private set; } = "{}";

    private UserPreference(Guid id) : base(id) { }

    /// <summary>
    /// Creates empty preferences for a user.
    /// </summary>
    public static Result<UserPreference> Create(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Result.Failure<UserPreference>(ErrorCodes.UserPreference.InvalidUserId);
        }

        return Result.Success(new UserPreference(Guid.NewGuid()) { UserId = userId.Trim() });
    }

    /// <summary>
    /// Updates all preference payloads after validating size limits.
    /// </summary>
    public Result Update(string shortcutsJson, string savedFiltersJson, string columnLayoutsJson)
    {
        if (!ValidateJsonSize(shortcutsJson) || !ValidateJsonSize(savedFiltersJson) || !ValidateJsonSize(columnLayoutsJson))
        {
            return Result.Failure(ErrorCodes.UserPreference.PayloadTooLarge);
        }

        ShortcutsJson = string.IsNullOrWhiteSpace(shortcutsJson) ? "{}" : shortcutsJson;
        SavedFiltersJson = string.IsNullOrWhiteSpace(savedFiltersJson) ? "{}" : savedFiltersJson;
        ColumnLayoutsJson = string.IsNullOrWhiteSpace(columnLayoutsJson) ? "{}" : columnLayoutsJson;
        return Result.Success();
    }

    private static bool ValidateJsonSize(string json) =>
        string.IsNullOrEmpty(json) || json.Length <= MaxJsonLength;
}
