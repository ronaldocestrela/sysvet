using Core.Domain;

namespace Fiscal.Domain.Entities;

/// <summary>Carta de correção eletrônica (CC-e) linked to an NF-e.</summary>
public sealed class FiscalCorrectionLetter : Entity
{
    public Guid FiscalDocumentId { get; private set; }
    public int Sequence { get; private set; }
    public string CorrectionText { get; private set; } = string.Empty;
    public string? Protocol { get; private set; }
    public DateTimeOffset? TransmittedAt { get; private set; }

    private FiscalCorrectionLetter() { }

    /// <summary>Creates a CC-e draft before SEFAZ transmission.</summary>
    public static Result<FiscalCorrectionLetter> Create(Guid documentId, int sequence, string correctionText)
    {
        if (string.IsNullOrWhiteSpace(correctionText) || correctionText.Trim().Length < 15)
        {
            return Result.Failure<FiscalCorrectionLetter>(ErrorCodes.Document.CorrectionInvalidFields);
        }

        var forbidden = new[] { "valor", "cfop", "destinat" };
        var lower = correctionText.ToLowerInvariant();
        if (forbidden.Any(f => lower.Contains(f, StringComparison.Ordinal)))
        {
            return Result.Failure<FiscalCorrectionLetter>(ErrorCodes.Document.CorrectionInvalidFields);
        }

        return Result.Success(new FiscalCorrectionLetter
        {
            Id = Guid.NewGuid(),
            FiscalDocumentId = documentId,
            Sequence = sequence,
            CorrectionText = correctionText.Trim()
        });
    }

    /// <summary>Marks CC-e as accepted by SEFAZ.</summary>
    public void MarkTransmitted(string protocol)
    {
        Protocol = protocol;
        TransmittedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
