using Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace Clients.Infrastructure.Fiscal;

/// <summary>SQLite fiscal store for PDV NFC-e.</summary>
public sealed class OfflineFiscalStore : IFiscalStore
{
    private readonly OfflineDbContext _dbContext;
    private readonly Http.ApiClient _apiClient;
    private readonly Sync.ISyncConnectivity _connectivity;

    public OfflineFiscalStore(OfflineDbContext dbContext, Http.ApiClient apiClient, Sync.ISyncConnectivity connectivity)
    {
        _dbContext = dbContext;
        _apiClient = apiClient;
        _connectivity = connectivity;
    }

    public async Task<Result<bool>> RefreshPosBundleAsync(CancellationToken cancellationToken = default)
    {
        if (!_connectivity.IsOnline)
        {
            return Result.Success(false);
        }

        var bundle = await _apiClient.GetAsync<FiscalPosBundleClientDto?>("/api/v1/fiscal/issuer/pos-bundle", cancellationToken);
        if (bundle.IsFailure || bundle.Value is null)
        {
            return bundle.IsFailure ? Result.Failure<bool>(bundle.Error) : Result.Success(false);
        }

        var dto = bundle.Value;
        var row = await _dbContext.FiscalIssuerCache.FirstOrDefaultAsync(cancellationToken);
        if (row is null)
        {
            row = new OfflineFiscalIssuerCache();
            _dbContext.FiscalIssuerCache.Add(row);
        }

        row.LegalName = dto.LegalName;
        row.TradeName = dto.TradeName;
        row.Cnpj = dto.Cnpj;
        row.State = dto.State;
        row.IbgeCityCode = dto.IbgeCityCode;
        row.NfceSeries = dto.NfceSeries;
        row.HasCertificate = dto.HasCertificate;
        row.EncryptedPfxBase64 = dto.EncryptedPfxBase64;
        row.EncryptedCertificatePassword = dto.EncryptedCertificatePassword;
        row.UpdatedAt = DateTimeOffset.UtcNow;

        var sequence = await _dbContext.FiscalSequences.FirstOrDefaultAsync(cancellationToken);
        if (sequence is null)
        {
            _dbContext.FiscalSequences.Add(new OfflineFiscalSequence { NfceSeries = dto.NfceSeries });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(true);
    }

    public async Task<IReadOnlyList<OfflineFiscalDocument>> GetDocumentsByOrderAsync(
        Guid orderId,
        CancellationToken cancellationToken = default) =>
        await _dbContext.FiscalDocuments.AsNoTracking()
            .Where(d => d.OrderId == orderId)
            .OrderByDescending(d => d.UpdatedAt)
            .ToListAsync(cancellationToken);

    public Task<OfflineFiscalDocument?> GetDocumentAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.FiscalDocuments.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public async Task<int> GetDeviceNfceSeriesAsync(CancellationToken cancellationToken = default)
    {
        var sequence = await _dbContext.FiscalSequences.AsNoTracking().FirstOrDefaultAsync(cancellationToken);
        if (sequence is not null)
        {
            return sequence.NfceSeries;
        }

        var issuer = await _dbContext.FiscalIssuerCache.AsNoTracking().FirstOrDefaultAsync(cancellationToken);
        return issuer?.NfceSeries ?? 1;
    }

    public async Task SetDeviceNfceSeriesAsync(int series, CancellationToken cancellationToken = default)
    {
        if (series < 1 || series > 999)
        {
            return;
        }

        var sequence = await _dbContext.FiscalSequences.FirstOrDefaultAsync(cancellationToken);
        if (sequence is null)
        {
            sequence = new OfflineFiscalSequence { NfceSeries = series };
            _dbContext.FiscalSequences.Add(sequence);
        }
        else
        {
            sequence.NfceSeries = series;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
