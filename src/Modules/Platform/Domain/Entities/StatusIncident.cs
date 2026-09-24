using Core.Domain;

namespace Platform.Domain.Entities;

/// <summary>
/// Published operational incident for the public status API (ADR-060).
/// </summary>
public sealed class StatusIncident : Entity
{
    /// <summary>Short customer-facing title.</summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>Severity communicated on the status page.</summary>
    public StatusIncidentImpact Impact { get; private set; }

    /// <summary>Comma-separated component keys (e.g. api,sync,billing).</summary>
    public string Components { get; private set; } = string.Empty;

    /// <summary>When the incident started (UTC).</summary>
    public DateTimeOffset StartedAt { get; private set; }

    /// <summary>When the incident was resolved; null while open.</summary>
    public DateTimeOffset? ResolvedAt { get; private set; }

#pragma warning disable CS8618
    private StatusIncident()
    {
    }
#pragma warning restore CS8618

    /// <summary>Creates an open incident.</summary>
    public static Result<StatusIncident> Open(
        string titleRaw,
        StatusIncidentImpact impact,
        string componentsRaw,
        DateTimeOffset startedAtUtc)
    {
        var title = titleRaw?.Trim() ?? string.Empty;
        if (title.Length < 3 || title.Length > 256)
        {
            return Result.Failure<StatusIncident>(ErrorCodes.StatusIncident.InvalidTitle);
        }

        var components = NormalizeComponents(componentsRaw);
        if (components.IsFailure)
        {
            return Result.Failure<StatusIncident>(components.Error);
        }

        return Result.Success(new StatusIncident
        {
            Id = Guid.NewGuid(),
            Title = title,
            Impact = impact,
            Components = components.Value,
            StartedAt = startedAtUtc,
            UpdatedAt = startedAtUtc,
            RowVersion = new byte[8]
        });
    }

    /// <summary>Marks the incident resolved.</summary>
    public Result Resolve(DateTimeOffset resolvedAtUtc)
    {
        if (ResolvedAt is not null)
        {
            return Result.Failure(ErrorCodes.StatusIncident.AlreadyResolved);
        }

        if (resolvedAtUtc < StartedAt)
        {
            return Result.Failure(ErrorCodes.StatusIncident.InvalidResolutionTime);
        }

        ResolvedAt = resolvedAtUtc;
        UpdatedAt = resolvedAtUtc;
        return Result.Success();
    }

    /// <summary>True when still visible on the public status page.</summary>
    public bool IsOpen => ResolvedAt is null;

    private static Result<string> NormalizeComponents(string raw)
    {
        var parts = (raw ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(p => p.ToLowerInvariant())
            .Distinct()
            .ToList();

        if (parts.Count == 0)
        {
            return Result.Failure<string>(ErrorCodes.StatusIncident.InvalidComponents);
        }

        foreach (var part in parts)
        {
            if (part.Length > 32 || part.Any(c => !char.IsLetterOrDigit(c) && c != '-'))
            {
                return Result.Failure<string>(ErrorCodes.StatusIncident.InvalidComponents);
            }
        }

        return Result.Success(string.Join(',', parts));
    }
}
