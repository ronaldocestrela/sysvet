using Finance.Application.Titles.Dtos;
using Finance.Domain.Entities;

namespace Finance.Application.Titles;

internal static class FinancialTitleMapper
{
    public static FinancialTitleDto ToDto(FinancialTitle title) =>
        new()
        {
            Id = title.Id,
            Direction = title.Direction,
            Status = title.Status,
            SourceType = title.SourceType,
            SourceId = title.SourceId,
            SourceInstallmentKey = title.SourceInstallmentKey,
            PartyKind = title.PartyKind,
            PartyId = title.PartyId,
            CategoryId = title.CategoryId,
            CostCenterId = title.CostCenterId,
            IssueDate = title.IssueDate,
            DueDate = title.DueDate,
            OriginalAmount = title.OriginalAmount,
            SettledAmount = title.SettledAmount,
            OpenAmount = title.OpenAmount,
            Description = title.Description,
            Allocations = title.Allocations
                .Select(a => new TitleAllocationDto
                {
                    Id = a.Id,
                    Amount = a.Amount,
                    PaidAt = a.PaidAt,
                    Method = a.Method,
                    Kind = a.Kind
                })
                .ToList()
        };
}
