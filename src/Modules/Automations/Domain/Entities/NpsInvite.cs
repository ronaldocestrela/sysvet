using Automations.Domain.Enums;
using Core.Domain;

namespace Automations.Domain.Entities;

/// <summary>
/// Tokenized NPS survey invitation tied to a completed service.
/// </summary>
public sealed class NpsInvite : AggregateRoot
{
    public Guid TutorId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; private set; }
    public NpsInviteStatus Status { get; private set; }
    public int? Score { get; private set; }
    public string? Comment { get; private set; }
    public DateTimeOffset? RespondedAt { get; private set; }
    public string SourceType { get; private set; } = string.Empty;
    public Guid SourceId { get; private set; }
    public Guid? CampaignId { get; private set; }

    private NpsInvite() { }

    /// <summary>
    /// Creates a pending invite; the raw token is never persisted—only its hash.
    /// </summary>
    public static Result<NpsInvite> Create(
        Guid tutorId,
        string tokenHash,
        DateTimeOffset expiresAt,
        string sourceType,
        Guid sourceId,
        Guid? campaignId = null,
        Guid? id = null,
        DateTimeOffset? now = null)
    {
        if (tutorId == Guid.Empty)
        {
            return Result.Failure<NpsInvite>(ErrorCodes.Nps.InvalidTutor);
        }

        if (string.IsNullOrWhiteSpace(tokenHash))
        {
            return Result.Failure<NpsInvite>(ErrorCodes.Nps.InvalidToken);
        }

        if (string.IsNullOrWhiteSpace(sourceType))
        {
            return Result.Failure<NpsInvite>(ErrorCodes.Nps.InvalidSource);
        }

        if (sourceId == Guid.Empty)
        {
            return Result.Failure<NpsInvite>(ErrorCodes.Nps.InvalidSource);
        }

        var clock = now ?? DateTimeOffset.UtcNow;
        if (expiresAt <= clock)
        {
            return Result.Failure<NpsInvite>(ErrorCodes.Nps.InvalidExpiry);
        }

        return Result.Success(new NpsInvite
        {
            Id = id ?? Guid.NewGuid(),
            TutorId = tutorId,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt,
            Status = NpsInviteStatus.Pending,
            SourceType = sourceType.Trim(),
            SourceId = sourceId,
            CampaignId = campaignId
        });
    }

    /// <summary>
    /// Records promoter/passive/detractor score and optional verbatim comment.
    /// </summary>
    public Result Respond(int score, string? comment, DateTimeOffset now)
    {
        if (Status == NpsInviteStatus.Responded)
        {
            return Result.Failure(ErrorCodes.Nps.AlreadyResponded);
        }

        if (Status == NpsInviteStatus.Expired || now > ExpiresAt)
        {
            Status = NpsInviteStatus.Expired;
            return Result.Failure(ErrorCodes.Nps.Expired);
        }

        if (score is < 0 or > 10)
        {
            return Result.Failure(ErrorCodes.Nps.InvalidScore);
        }

        Score = score;
        Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
        RespondedAt = now;
        Status = NpsInviteStatus.Responded;
        return Result.Success();
    }

    /// <summary>
    /// Marks the invite expired when the tutor opens the link after the deadline.
    /// </summary>
    public void MarkExpired(DateTimeOffset now)
    {
        if (Status == NpsInviteStatus.Pending && now > ExpiresAt)
        {
            Status = NpsInviteStatus.Expired;
        }
    }
}
