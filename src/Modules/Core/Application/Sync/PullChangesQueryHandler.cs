using Core.Application.Common;
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
        var takeResult = PageRequest.TryNormalizeTake(request.Take);
        if (takeResult.IsFailure)
        {
            return Result.Failure<PullChangesResult>(takeResult.Error);
        }

        var page = await _changeFeedReader.ReadChangesAsync(request.Since, takeResult.Value, cancellationToken);
        return Result.Success(page);
    }
}
