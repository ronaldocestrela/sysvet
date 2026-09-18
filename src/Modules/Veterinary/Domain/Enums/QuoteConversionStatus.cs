namespace Veterinary.Domain.Enums;

/// <summary>PDV conversion lifecycle for an approved clinical quote.</summary>
public enum QuoteConversionStatus
{
    /// <summary>Not applicable (draft, sent, or rejected).</summary>
    None = 0,

    /// <summary>Approved and waiting for sale conversion (Fase 6 inbox).</summary>
    Pending = 1,

    /// <summary>Linked to a paid or draft order after PDV conversion.</summary>
    Converted = 2
}
