using System.Text.Json;
using Core.Domain;
using Core.Domain.Auditing;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Core.Infrastructure.Persistence;

/// <summary>
/// Builds sanitized audit payloads and resolves semantic actions for change-tracked entities.
/// </summary>
internal static class AuditCaptureHelper
{
    private const int MaxPayloadLength = 2048;

    private static readonly HashSet<string> ExcludedPropertyNames = new(StringComparer.Ordinal)
    {
        "PasswordHash",
        "SecurityStamp",
        "ConcurrencyStamp",
        "NormalizedEmail",
        "NormalizedUserName",
        "PermissionCodesStorage",
        "PermissionCodesJson"
    };

    /// <summary>
    /// Returns whether the entity type participates in automatic audit capture.
    /// </summary>
    public static bool ShouldAudit(EntityEntry entry) =>
        entry.Entity is IAuditable and not AuditLog;

    /// <summary>
    /// Resolves the audit action name, mapping soft deletes to <c>Deleted</c>.
    /// </summary>
    public static string ResolveAction(EntityEntry entry)
    {
        if (entry.Entity is ISoftDeletable softDeletable
            && entry.State == EntityState.Modified
            && softDeletable.IsDeleted
            && entry.Property(nameof(ISoftDeletable.IsDeleted)).IsModified)
        {
            return nameof(EntityState.Deleted);
        }

        return entry.State.ToString();
    }

    /// <summary>
    /// Builds a JSON summary of scalar properties for the audit entry.
    /// </summary>
    public static string BuildPayloadSummary(EntityEntry entry)
    {
        var values = new Dictionary<string, object?>(StringComparer.Ordinal);

        foreach (var property in entry.Properties)
        {
            if (property.Metadata.IsShadowProperty())
            {
                continue;
            }

            if (ExcludedPropertyNames.Contains(property.Metadata.Name))
            {
                continue;
            }

            if (entry.State == EntityState.Modified && !property.IsModified)
            {
                continue;
            }

            var value = entry.State == EntityState.Deleted
                ? property.OriginalValue
                : property.CurrentValue;

            if (value is null)
            {
                values[property.Metadata.Name] = null;
                continue;
            }

            var type = value.GetType();
            if (type.IsClass && type != typeof(string) && !type.IsEnum)
            {
                values[property.Metadata.Name] = value.ToString();
            }
            else
            {
                values[property.Metadata.Name] = value;
            }
        }

        var json = JsonSerializer.Serialize(values);
        if (json.Length <= MaxPayloadLength)
        {
            return json;
        }

        return json[..MaxPayloadLength];
    }

    /// <summary>
    /// Gets the primary key value as <see cref="Guid"/> for auditable entities.
    /// </summary>
    public static Guid GetEntityId(EntityEntry entry)
    {
        if (entry.Entity is Entity entity)
        {
            return entity.Id;
        }

        var key = entry.Metadata.FindPrimaryKey();
        if (key?.Properties.Count == 1)
        {
            var value = entry.Property(key.Properties[0].Name).CurrentValue
                ?? entry.Property(key.Properties[0].Name).OriginalValue;
            if (value is Guid guid)
            {
                return guid;
            }
        }

        return Guid.Empty;
    }

    /// <summary>
    /// Gets the CLR type name used as the audit entity name.
    /// </summary>
    public static string GetEntityName(EntityEntry entry) => entry.Metadata.ClrType.Name;
}
