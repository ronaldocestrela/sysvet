namespace Platform.Domain.Entities;

/// <summary>Outbound channel for SaaS dunning notices (9.5).</summary>
public enum DunningChannel
{
    /// <summary>E-mail to billing contact.</summary>
    Email = 0,

    /// <summary>SMS when provider configured.</summary>
    Sms = 1
}
