using Core.Domain;
using MediatR;

namespace Core.Application.Sync;

/// <summary>
/// Handles pull requests by delegating to <see cref="ISyncChangeFeedReader"/>.
/// </summary>
public sealed class PullChangesQueryHandler : IRequestHandler<PullChangesQuery, Result<PullChangesResult>>
{
    private readonly ISyncChangeFeedReader _changeFeedReader;

    /// <summary>
    /// Creates the handler.
    /// </summary>
    public PullChangesQueryHandler(ISyncChangeFeedReader changeFeedReader)
    {
        _changeFeedReader = changeFeedReader;
    }

    /// <inheritdoc />
    public async Task<Result<PullChangesResult>> Handle(PullChangesQuery request, CancellationToken cancellationToken)
    {
        var take = request.Take <= 0 ? 100 : Math.Min(request.Take, 500);
        var page = await _changeFeedReader.ReadChangesAsync(request.Since, take, cancellationToken);
        return Result.Success(page);
    }
}
