using Fiscal.Domain.Enums;

namespace Fiscal.Application.Documents;

/// <summary>Fiscal document list item.</summary>
public sealed record FiscalDocumentDto(
    Guid Id,
    FiscalDocumentType DocumentType,
    FiscalDocumentStatus Status,
    Guid SourceOrderId,
    string? AccessKey,
    decimal TotalAmount,
    DateTimeOffset CreatedAt,
    DateTimeOffset? AuthorizedAt);

/// <summary>Fiscal document detail.</summary>
public sealed record FiscalDocumentDetailDto(
    Guid Id,
    FiscalDocumentType DocumentType,
    FiscalDocumentStatus Status,
    Guid SourceOrderId,
    string? AccessKey,
    string? Protocol,
    string? RejectionReason,
    string RecipientName,
    decimal TotalAmount,
    IReadOnlyList<FiscalDocumentLineDto> Lines,
    IReadOnlyList<FiscalCorrectionDto> Corrections);

public sealed record FiscalDocumentLineDto(
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    string? Ncm,
    string? Cfop);

public sealed record FiscalCorrectionDto(int Sequence, string Text, string? Protocol);

/// <summary>Binary download for XML/DANFE.</summary>
public sealed record FiscalFileDownloadDto(string FileName, string ContentType, byte[] Content);
