namespace Finance.Domain.Enums;

/// <summary>Lifecycle of a financial title.</summary>
public enum TitleStatus
{
    Open = 0,
    PartiallySettled = 1,
    Settled = 2,
    Cancelled = 3
}
