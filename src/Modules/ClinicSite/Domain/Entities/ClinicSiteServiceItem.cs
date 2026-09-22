using Core.Domain;

namespace ClinicSite.Domain.Entities;

/// <summary>
/// A service line displayed on the public clinic site.
/// </summary>
public sealed class ClinicSiteServiceItem : Entity
{
    public Guid ClinicSiteProfileId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int? DurationMinutes { get; private set; }
    public decimal? Price { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsVisible { get; private set; } = true;

#pragma warning disable CS8618
    private ClinicSiteServiceItem() : base(Guid.Empty) { }
#pragma warning restore CS8618

    private ClinicSiteServiceItem(Guid id, Guid profileId, string name, string? description, int? durationMinutes, decimal? price, int sortOrder, bool isVisible)
        : base(id)
    {
        ClinicSiteProfileId = profileId;
        Name = name;
        Description = description;
        DurationMinutes = durationMinutes;
        Price = price;
        SortOrder = sortOrder;
        IsVisible = isVisible;
    }

    /// <summary>
    /// Creates a service row linked to the site profile.
    /// </summary>
    public static Result<ClinicSiteServiceItem> Create(
        Guid profileId,
        Guid id,
        string name,
        string? description,
        int? durationMinutes,
        decimal? price,
        int sortOrder,
        bool isVisible)
    {
        if (profileId == Guid.Empty)
        {
            return Result.Failure<ClinicSiteServiceItem>(ErrorCodes.Site.NotFound);
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<ClinicSiteServiceItem>(ErrorCodes.Site.PublishIncomplete);
        }

        if (durationMinutes is <= 0)
        {
            durationMinutes = null;
        }

        if (price is < 0)
        {
            return Result.Failure<ClinicSiteServiceItem>(ErrorCodes.Site.PublishIncomplete);
        }

        return Result.Success(new ClinicSiteServiceItem(
            id == Guid.Empty ? Guid.NewGuid() : id,
            profileId,
            name.Trim(),
            string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            durationMinutes,
            price,
            sortOrder,
            isVisible));
    }
}
