using Core.Application.Sync;
using Fiscal.Domain.Enums;
using Fiscal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fiscal.Infrastructure.Sync;

/// <summary>Exposes issuer profile and NFC-e documents for sync pull.</summary>
public sealed class FiscalSyncChangeFeedContributor : ISyncChangeFeedContributor
{
    private readonly FiscalDbContext _dbContext;

    public FiscalSyncChangeFeedContributor(FiscalDbContext dbContext) => _dbContext = dbContext;

    public async Task<SyncContributorChanges> ReadChangesAsync(DateTimeOffset since, int take, CancellationToken cancellationToken)
    {
        var issuer = await _dbContext.IssuerProfiles.AsNoTracking().FirstOrDefaultAsync(cancellationToken);
        var issuerDtos = new List<SyncFiscalIssuerDto>();
        var maxUpdated = since;

        if (issuer is not null && issuer.UpdatedAt > since)
        {
            issuerDtos.Add(new SyncFiscalIssuerDto
            {
                Id = issuer.Id,
                LegalName = issuer.LegalName,
                TradeName = issuer.TradeName,
                Cnpj = issuer.Cnpj.Value,
                State = issuer.State,
                IbgeCityCode = issuer.IbgeCityCode,
                NfceSeries = issuer.NfceSeries,
                NfeSeries = issuer.NfeSeries,
                Environment = issuer.Environment.ToString(),
                HasCertificate = issuer.HasCertificate,
                UpdatedAt = issuer.UpdatedAt
            });
            maxUpdated = issuer.UpdatedAt;
        }

        // SQLite in CI does not translate DateTimeOffset filters/orderings reliably.
        var documentCandidates = await _dbContext.FiscalDocuments
            .AsNoTracking()
            .Where(d => d.DocumentType == FiscalDocumentType.Nfce)
            .ToListAsync(cancellationToken);

        var documents = documentCandidates
            .Where(d => d.UpdatedAt > since)
            .OrderBy(d => d.UpdatedAt)
            .Take(take + 1)
            .ToList();

        var hasMore = documents.Count > take;
        if (hasMore)
        {
            documents = documents.Take(take).ToList();
        }

        foreach (var doc in documents)
        {
            if (doc.UpdatedAt > maxUpdated)
            {
                maxUpdated = doc.UpdatedAt;
            }
        }

        return new SyncContributorChanges
        {
            FiscalIssuers = issuerDtos,
            FiscalDocuments = documents.Select(d => new SyncFiscalDocumentDto
            {
                Id = d.Id,
                DocumentType = d.DocumentType.ToString(),
                Status = d.Status.ToString(),
                SourceOrderId = d.SourceOrderId,
                AccessKey = d.AccessKey,
                Protocol = d.Protocol,
                RejectionReason = d.RejectionReason,
                EmissionType = d.EmissionType?.ToString(),
                QrCodeUrl = d.QrCodeUrl,
                NfeNumber = d.NfeNumber,
                NfeSeries = d.NfeSeries,
                RecipientName = d.RecipientName,
                RecipientCpf = d.RecipientCpf,
                UpdatedAt = d.UpdatedAt,
                AuthorizedAt = d.AuthorizedAt
            }).ToList(),
            MaxUpdatedAt = maxUpdated,
            HasMore = hasMore
        };
    }
}
