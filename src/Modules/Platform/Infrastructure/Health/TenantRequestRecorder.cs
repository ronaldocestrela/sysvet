using Platform.Application.Abstractions;
using Platform.Domain.Repositories;

namespace Platform.Infrastructure.Health;

/// <summary>Persists daily tenant request counters (9.7).</summary>
public sealed class TenantRequestRecorder : ITenantRequestRecorder
{
    private readonly ITenantRequestDailyRepository _repository;
    private readonly IPlatformUnitOfWork _unitOfWork;

    /// <summary>Creates the recorder.</summary>
    public TenantRequestRecorder(ITenantRequestDailyRepository repository, IPlatformUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task RecordAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        var row = await _repository.GetOrCreateAsync(tenantId, now, cancellationToken);
        row.Increment(now);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
