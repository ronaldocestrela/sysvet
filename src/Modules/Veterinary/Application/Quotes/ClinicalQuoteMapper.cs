using Core.Domain;
using Veterinary.Application.Quotes.Dtos;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Enums;
using VetErrors = Veterinary.Domain.ErrorCodes;

namespace Veterinary.Application.Quotes;

/// <summary>Maps clinical quote aggregates to API DTOs.</summary>
public static class ClinicalQuoteMapper
{
    /// <summary>Maps aggregate to detail DTO.</summary>
    public static ClinicalQuoteDto ToDto(ClinicalQuote quote) =>
        new()
        {
            Id = quote.Id,
            AppointmentId = quote.AppointmentId,
            PetId = quote.PetId,
            TutorId = quote.TutorId,
            Status = quote.Status.ToString(),
            ConversionStatus = quote.ConversionStatus.ToString(),
            ConvertedOrderId = quote.ConvertedOrderId,
            Notes = quote.Notes,
            SentAt = quote.SentAt,
            DecidedAt = quote.DecidedAt,
            TotalAmount = quote.TotalAmount,
            Items = quote.Items.OrderBy(i => i.SortOrder).Select(ToItemDto).ToList(),
            UpdatedAt = quote.UpdatedAt
        };

    /// <summary>Maps aggregate to list summary.</summary>
    public static ClinicalQuoteListItemDto ToListItem(ClinicalQuote quote) =>
        new()
        {
            Id = quote.Id,
            AppointmentId = quote.AppointmentId,
            Status = quote.Status.ToString(),
            ConversionStatus = quote.ConversionStatus.ToString(),
            TotalAmount = quote.TotalAmount,
            ItemCount = quote.Items.Count,
            UpdatedAt = quote.UpdatedAt
        };

    /// <summary>Maps line entity to DTO.</summary>
    public static ClinicalQuoteItemDto ToItemDto(ClinicalQuoteItem item) =>
        new()
        {
            Id = item.Id,
            Description = item.Description,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            Kind = item.Kind.ToString(),
            ProductId = item.ProductId,
            SortOrder = item.SortOrder,
            LineTotal = item.LineTotal
        };

    /// <summary>Parses item kind from API input.</summary>
    public static Result<ClinicalQuoteItemKind> ParseKind(string? kind)
    {
        if (string.IsNullOrWhiteSpace(kind))
        {
            return Result.Success(ClinicalQuoteItemKind.Service);
        }

        return Enum.TryParse<ClinicalQuoteItemKind>(kind, true, out var parsed)
            ? Result.Success(parsed)
            : Result.Failure<ClinicalQuoteItemKind>(VetErrors.ClinicalQuote.InvalidDescription);
    }
}
