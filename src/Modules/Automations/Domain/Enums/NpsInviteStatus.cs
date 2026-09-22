namespace Automations.Domain.Enums;

/// <summary>
/// State of an outbound NPS survey invitation.
/// </summary>
public enum NpsInviteStatus
{
    Pending = 1,
    Responded = 2,
    Expired = 3
}
