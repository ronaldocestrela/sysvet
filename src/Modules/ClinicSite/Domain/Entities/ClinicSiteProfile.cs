using Core.Domain;

namespace ClinicSite.Domain.Entities;

/// <summary>
/// Tenant singleton public marketing site content (services, team, hours managed as child rows).
/// </summary>
public sealed class ClinicSiteProfile : AggregateRoot
{
    public const string SingletonKey = "default";

    /// <summary>Fixed key so only one profile exists per tenant schema.</summary>
    public string Key { get; private set; } = SingletonKey;

    public string DisplayName { get; private set; } = string.Empty;
    public string? Tagline { get; private set; }
    public string Street { get; private set; } = string.Empty;
    public string Number { get; private set; } = string.Empty;
    public string? Complement { get; private set; }
    public string District { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string State { get; private set; } = string.Empty;
    public string PostalCode { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? WhatsApp { get; private set; }
    public string? LogoUrl { get; private set; }
    public string Slug { get; private set; } = string.Empty;
    public bool IsPublished { get; private set; }

    private readonly List<ClinicSiteServiceItem> _services = new();
    private readonly List<ClinicSiteTeamMember> _team = new();
    private readonly List<ClinicSiteOpeningHours> _hours = new();

    public IReadOnlyCollection<ClinicSiteServiceItem> Services => _services.AsReadOnly();
    public IReadOnlyCollection<ClinicSiteTeamMember> Team => _team.AsReadOnly();
    public IReadOnlyCollection<ClinicSiteOpeningHours> Hours => _hours.AsReadOnly();

#pragma warning disable CS8618
    private ClinicSiteProfile() : base(Guid.Empty) { }
#pragma warning restore CS8618

    private ClinicSiteProfile(Guid id)
        : base(id)
    {
        Key = SingletonKey;
    }

    /// <summary>
    /// Creates the default empty profile for a tenant.
    /// </summary>
    public static Result<ClinicSiteProfile> CreateDefault(Guid? id = null) =>
        Result.Success(new ClinicSiteProfile(id ?? Guid.NewGuid()));

    /// <summary>
    /// Updates cadastral and contact fields shown on the public site.
    /// </summary>
    public Result UpdateProfile(
        string displayName,
        string? tagline,
        string street,
        string number,
        string? complement,
        string district,
        string city,
        string state,
        string postalCode,
        string phone,
        string email,
        string? whatsApp,
        string? logoUrl)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return Result.Failure(ErrorCodes.Site.PublishIncomplete);
        }

        DisplayName = displayName.Trim();
        Tagline = string.IsNullOrWhiteSpace(tagline) ? null : tagline.Trim();
        Street = street?.Trim() ?? string.Empty;
        Number = number?.Trim() ?? string.Empty;
        Complement = string.IsNullOrWhiteSpace(complement) ? null : complement.Trim();
        District = district?.Trim() ?? string.Empty;
        City = city?.Trim() ?? string.Empty;
        State = (state ?? string.Empty).Trim().ToUpperInvariant();
        PostalCode = new string((postalCode ?? string.Empty).Where(char.IsDigit).ToArray());
        Phone = phone?.Trim() ?? string.Empty;
        Email = email?.Trim() ?? string.Empty;
        WhatsApp = string.IsNullOrWhiteSpace(whatsApp) ? null : whatsApp.Trim();
        LogoUrl = string.IsNullOrWhiteSpace(logoUrl) ? null : logoUrl.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    /// <summary>
    /// Assigns the public slug (normalized); does not publish by itself.
    /// </summary>
    public Result SetSlug(string slug)
    {
        var normalized = ValueObjects.PublicSiteSlug.Normalize(slug);
        if (!ValueObjects.PublicSiteSlug.IsValid(normalized))
        {
            return Result.Failure(ErrorCodes.Site.InvalidSlug);
        }

        Slug = normalized;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    /// <summary>
    /// Replaces visible service lines for the public services page.
    /// </summary>
    public Result ReplaceServices(IEnumerable<(Guid Id, string Name, string? Description, int? DurationMinutes, decimal? Price, int SortOrder, bool IsVisible)> lines)
    {
        _services.Clear();
        foreach (var line in lines)
        {
            var item = ClinicSiteServiceItem.Create(Id, line.Id, line.Name, line.Description, line.DurationMinutes, line.Price, line.SortOrder, line.IsVisible);
            if (item.IsFailure)
            {
                return Result.Failure(item.Error);
            }

            _services.Add(item.Value);
        }

        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    /// <summary>
    /// Replaces team members for the public team page.
    /// </summary>
    public Result ReplaceTeam(IEnumerable<(Guid Id, string Name, string RoleTitle, string? Bio, int SortOrder, bool IsVisible)> members)
    {
        _team.Clear();
        foreach (var member in members)
        {
            var item = ClinicSiteTeamMember.Create(Id, member.Id, member.Name, member.RoleTitle, member.Bio, member.SortOrder, member.IsVisible);
            if (item.IsFailure)
            {
                return Result.Failure(item.Error);
            }

            _team.Add(item.Value);
        }

        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    /// <summary>
    /// Replaces weekly opening hours.
    /// </summary>
    public Result ReplaceHours(IEnumerable<(Guid Id, DayOfWeek Day, TimeOnly? OpenTime, TimeOnly? CloseTime, bool IsClosed)> rows)
    {
        _hours.Clear();
        foreach (var row in rows)
        {
            var item = ClinicSiteOpeningHours.Create(Id, row.Id, row.Day, row.OpenTime, row.CloseTime, row.IsClosed);
            if (item.IsFailure)
            {
                return Result.Failure(item.Error);
            }

            _hours.Add(item.Value);
        }

        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    /// <summary>
    /// Validates publish prerequisites and marks the profile published.
    /// </summary>
    public Result Publish()
    {
        if (string.IsNullOrWhiteSpace(Slug) || !ValueObjects.PublicSiteSlug.IsValid(Slug))
        {
            return Result.Failure(ErrorCodes.Site.InvalidSlug);
        }

        if (string.IsNullOrWhiteSpace(DisplayName))
        {
            return Result.Failure(ErrorCodes.Site.PublishIncomplete);
        }

        if (string.IsNullOrWhiteSpace(Phone) && string.IsNullOrWhiteSpace(Email))
        {
            return Result.Failure(ErrorCodes.Site.PublishIncomplete);
        }

        IsPublished = true;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    /// <summary>
    /// Hides the site from public visitors without clearing content.
    /// </summary>
    public Result Unpublish()
    {
        IsPublished = false;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }
}
