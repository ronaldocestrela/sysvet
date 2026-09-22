using Core.Domain;

namespace ClinicSite.Domain.Entities;

/// <summary>
/// Weekly opening hours row for the public site.
/// </summary>
public sealed class ClinicSiteOpeningHours : Entity
{
    public Guid ClinicSiteProfileId { get; private set; }
    public DayOfWeek Day { get; private set; }
    public TimeOnly? OpenTime { get; private set; }
    public TimeOnly? CloseTime { get; private set; }
    public bool IsClosed { get; private set; }

#pragma warning disable CS8618
    private ClinicSiteOpeningHours() : base(Guid.Empty) { }
#pragma warning restore CS8618

    private ClinicSiteOpeningHours(Guid id, Guid profileId, DayOfWeek day, TimeOnly? openTime, TimeOnly? closeTime, bool isClosed)
        : base(id)
    {
        ClinicSiteProfileId = profileId;
        Day = day;
        OpenTime = openTime;
        CloseTime = closeTime;
        IsClosed = isClosed;
    }

    /// <summary>
    /// Creates an opening hours row for one weekday.
    /// </summary>
    public static Result<ClinicSiteOpeningHours> Create(
        Guid profileId,
        Guid id,
        DayOfWeek day,
        TimeOnly? openTime,
        TimeOnly? closeTime,
        bool isClosed)
    {
        if (profileId == Guid.Empty)
        {
            return Result.Failure<ClinicSiteOpeningHours>(ErrorCodes.Site.NotFound);
        }

        if (!isClosed && (openTime is null || closeTime is null || openTime >= closeTime))
        {
            return Result.Failure<ClinicSiteOpeningHours>(ErrorCodes.Site.PublishIncomplete);
        }

        return Result.Success(new ClinicSiteOpeningHours(
            id == Guid.Empty ? Guid.NewGuid() : id,
            profileId,
            day,
            isClosed ? null : openTime,
            isClosed ? null : closeTime,
            isClosed));
    }
}
