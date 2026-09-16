namespace Core.Domain.Authorization;

/// <summary>
/// Value object wrapping a catalog permission string so invalid codes cannot enter the domain.
/// </summary>
public sealed record PermissionCode : ValueObject
{
    /// <summary>
    /// Canonical permission identifier (e.g. <c>Tutors.Read</c>).
    /// </summary>
    public string Value { get; }

    private PermissionCode(string value) => Value = value;

    /// <summary>
    /// Creates a permission code when it exists in the static catalog.
    /// </summary>
    public static Result<PermissionCode> Create(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Result.Failure<PermissionCode>(ErrorCodes.AccessProfile.InvalidPermission);
        }

        var normalized = code.Trim();
        if (!Permissions.IsValid(normalized))
        {
            return Result.Failure<PermissionCode>(ErrorCodes.AccessProfile.InvalidPermission);
        }

        return Result.Success(new PermissionCode(normalized));
    }
}
