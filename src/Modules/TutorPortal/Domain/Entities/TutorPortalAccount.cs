using Core.Domain;

namespace TutorPortal.Domain.Entities;

/// <summary>
/// Links an ASP.NET Identity user with role <c>Tutor</c> to a CRM <see cref="Core.Domain.Entities.Tutor"/> aggregate within a tenant.
/// </summary>
public sealed class TutorPortalAccount : Entity
{
    /// <summary>
    /// Identity user identifier (<c>AppUser.Id</c>).
    /// </summary>
    public string UserId { get; private set; } = string.Empty;

    /// <summary>
    /// CRM tutor aggregate identifier.
    /// </summary>
    public Guid TutorId { get; private set; }

#pragma warning disable CS8618
    private TutorPortalAccount() : base(Guid.Empty) { }
#pragma warning restore CS8618

    private TutorPortalAccount(Guid id, string userId, Guid tutorId)
        : base(id)
    {
        UserId = userId;
        TutorId = tutorId;
    }

    /// <summary>
    /// Creates a new portal account binding when both identifiers are non-empty and distinct from existing rows (enforced at persistence).
    /// </summary>
    public static Result<TutorPortalAccount> Create(string userId, Guid tutorId, Guid id = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Result.Failure<TutorPortalAccount>(ErrorCodes.Account.NotFound);
        }

        if (tutorId == Guid.Empty)
        {
            return Result.Failure<TutorPortalAccount>(ErrorCodes.Account.NotFound);
        }

        return Result.Success(new TutorPortalAccount(id == Guid.Empty ? Guid.NewGuid() : id, userId.Trim(), tutorId));
    }
}
