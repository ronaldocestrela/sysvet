using Core.Domain;

namespace ClinicSite.Domain.Entities;

/// <summary>
/// A team member shown on the public clinic site.
/// </summary>
public sealed class ClinicSiteTeamMember : Entity
{
    public Guid ClinicSiteProfileId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string RoleTitle { get; private set; } = string.Empty;
    public string? Bio { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsVisible { get; private set; } = true;

#pragma warning disable CS8618
    private ClinicSiteTeamMember() : base(Guid.Empty) { }
#pragma warning restore CS8618

    private ClinicSiteTeamMember(Guid id, Guid profileId, string name, string roleTitle, string? bio, int sortOrder, bool isVisible)
        : base(id)
    {
        ClinicSiteProfileId = profileId;
        Name = name;
        RoleTitle = roleTitle;
        Bio = bio;
        SortOrder = sortOrder;
        IsVisible = isVisible;
    }

    /// <summary>
    /// Creates a team member row linked to the site profile.
    /// </summary>
    public static Result<ClinicSiteTeamMember> Create(
        Guid profileId,
        Guid id,
        string name,
        string roleTitle,
        string? bio,
        int sortOrder,
        bool isVisible)
    {
        if (profileId == Guid.Empty)
        {
            return Result.Failure<ClinicSiteTeamMember>(ErrorCodes.Site.NotFound);
        }

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(roleTitle))
        {
            return Result.Failure<ClinicSiteTeamMember>(ErrorCodes.Site.PublishIncomplete);
        }

        return Result.Success(new ClinicSiteTeamMember(
            id == Guid.Empty ? Guid.NewGuid() : id,
            profileId,
            name.Trim(),
            roleTitle.Trim(),
            string.IsNullOrWhiteSpace(bio) ? null : bio.Trim(),
            sortOrder,
            isVisible));
    }
}
