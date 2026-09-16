using System.Text.Json;
using Core.Domain.Authorization;

namespace Core.Domain.Entities;

/// <summary>
/// Tenant-scoped access profile holding a permission matrix and optional link to an Identity base role.
/// </summary>
public sealed class AccessProfile : AggregateRoot, IAuditable
{
    private readonly HashSet<string> _permissionCodes = new(StringComparer.Ordinal);
    private string _permissionCodesJson = "[]";

    /// <summary>
    /// Display name unique within the tenant.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Optional description for administrators.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// When true, the profile is seeded per tenant and cannot be deleted; name and base role are fixed.
    /// </summary>
    public bool IsSystem { get; private set; }

    /// <summary>
    /// Identity role synchronized when users are assigned this profile (<see cref="ApplicationRoles"/>).
    /// </summary>
    public string BaseRole { get; private set; } = string.Empty;

    /// <summary>
    /// Granted permission codes from the static catalog.
    /// </summary>
    public IReadOnlyCollection<string> PermissionCodes => _permissionCodes;

    /// <summary>
    /// JSON persistence column mapped by EF Core (not for application use).
    /// </summary>
    public string PermissionCodesStorage
    {
        get => _permissionCodesJson;
        private set
        {
            _permissionCodesJson = string.IsNullOrWhiteSpace(value) ? "[]" : value;
            LoadPermissions(JsonSerializer.Deserialize<List<string>>(_permissionCodesJson) ?? []);
        }
    }

    private AccessProfile(Guid id) : base(id) { }

    /// <summary>
    /// Creates a system profile seeded for each tenant.
    /// </summary>
    public static Result<AccessProfile> CreateSystem(string name, string baseRole, IEnumerable<string> permissionCodes)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<AccessProfile>(ErrorCodes.AccessProfile.InvalidName);
        }

        if (string.IsNullOrWhiteSpace(baseRole))
        {
            return Result.Failure<AccessProfile>(ErrorCodes.AccessProfile.InvalidBaseRole);
        }

        var profile = new AccessProfile(Guid.NewGuid())
        {
            Name = name.Trim(),
            IsSystem = true,
            BaseRole = baseRole.Trim()
        };

        foreach (var code in permissionCodes)
        {
            var grant = profile.Grant(code);
            if (grant.IsFailure)
            {
                return Result.Failure<AccessProfile>(grant.Error);
            }
        }

        return Result.Success(profile);
    }

    /// <summary>
    /// Creates a custom profile cloned from permissions of another profile.
    /// </summary>
    public static Result<AccessProfile> CreateCustom(string name, string? description, string baseRole, IEnumerable<string> permissionCodes)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<AccessProfile>(ErrorCodes.AccessProfile.InvalidName);
        }

        if (string.IsNullOrWhiteSpace(baseRole))
        {
            return Result.Failure<AccessProfile>(ErrorCodes.AccessProfile.InvalidBaseRole);
        }

        var profile = new AccessProfile(Guid.NewGuid())
        {
            Name = name.Trim(),
            Description = description?.Trim(),
            IsSystem = false,
            BaseRole = baseRole.Trim()
        };

        foreach (var code in permissionCodes)
        {
            var grant = profile.Grant(code);
            if (grant.IsFailure)
            {
                return Result.Failure<AccessProfile>(grant.Error);
            }
        }

        return Result.Success(profile);
    }

    /// <summary>
    /// Clones this profile into a new non-system profile with a new name.
    /// </summary>
    public Result<AccessProfile> Clone(string newName)
    {
        return CreateCustom(newName, Description, BaseRole, _permissionCodes.ToList());
    }

    /// <summary>
    /// Renames a custom profile; system profiles cannot be renamed.
    /// </summary>
    public Result Rename(string newName)
    {
        if (IsSystem)
        {
            return Result.Failure(ErrorCodes.AccessProfile.CannotRenameSystem);
        }

        if (string.IsNullOrWhiteSpace(newName))
        {
            return Result.Failure(ErrorCodes.AccessProfile.InvalidName);
        }

        Name = newName.Trim();
        return Result.Success();
    }

    /// <summary>
    /// Updates description (allowed for system and custom profiles).
    /// </summary>
    public void SetDescription(string? description) => Description = description?.Trim();

    /// <summary>
    /// Grants a catalog permission idempotently.
    /// </summary>
    public Result Grant(string permissionCode)
    {
        var parsed = PermissionCode.Create(permissionCode);
        if (parsed.IsFailure)
        {
            return Result.Failure(parsed.Error);
        }

        _permissionCodes.Add(parsed.Value.Value);
        SyncPermissionJson();
        return Result.Success();
    }

    /// <summary>
    /// Revokes a catalog permission idempotently.
    /// </summary>
    public Result Revoke(string permissionCode)
    {
        var parsed = PermissionCode.Create(permissionCode);
        if (parsed.IsFailure)
        {
            return Result.Failure(parsed.Error);
        }

        _permissionCodes.Remove(parsed.Value.Value);
        SyncPermissionJson();
        return Result.Success();
    }

    /// <summary>
    /// Replaces the entire permission set with a validated catalog subset.
    /// </summary>
    public Result SetPermissions(IEnumerable<string> permissionCodes)
    {
        var list = permissionCodes.ToList();
        foreach (var code in list)
        {
            if (!Permissions.IsValid(code))
            {
                return Result.Failure(ErrorCodes.AccessProfile.InvalidPermission);
            }
        }

        _permissionCodes.Clear();
        foreach (var code in list)
        {
            _permissionCodes.Add(code);
        }

        SyncPermissionJson();
        return Result.Success();
    }

    /// <summary>
    /// Marks the profile for deletion when business rules allow.
    /// </summary>
    public Result MarkForDeletion()
    {
        if (IsSystem)
        {
            return Result.Failure(ErrorCodes.AccessProfile.CannotDeleteSystem);
        }

        return Result.Success();
    }

    /// <summary>
    /// EF Core materialization of permission codes from storage.
    /// </summary>
    internal void LoadPermissions(IEnumerable<string> codes)
    {
        _permissionCodes.Clear();
        foreach (var code in codes)
        {
            _permissionCodes.Add(code);
        }
    }

    private void SyncPermissionJson()
    {
        _permissionCodesJson = JsonSerializer.Serialize(_permissionCodes.OrderBy(c => c, StringComparer.Ordinal).ToList());
    }
}
