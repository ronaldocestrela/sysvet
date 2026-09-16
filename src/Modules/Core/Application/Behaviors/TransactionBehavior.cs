using Core.Application.Common.Interfaces;
using Core.Application.Messaging;
using Core.Domain;
using MediatR;

namespace Core.Application.Behaviors;

/// <summary>
/// Commits all registered unit-of-work instances after successful commands and dispatches domain events.
/// </summary>
public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IUnitOfWork> _unitOfWorks;
    private readonly IEnumerable<IDomainEventSource> _domainEventSources;
    private readonly IDomainEventDispatcher _domainEventDispatcher;

    public TransactionBehavior(
        IEnumerable<IUnitOfWork> unitOfWorks,
        IEnumerable<IDomainEventSource> domainEventSources,
        IDomainEventDispatcher domainEventDispatcher)
    {
        _unitOfWorks = unitOfWorks;
        _domainEventSources = domainEventSources;
        _domainEventDispatcher = domainEventDispatcher;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not ICommandBase)
        {
            return await next();
        }

        var response = await next();

        if (response is Result result && !result.IsSuccess)
        {
            return response;
        }

        foreach (var unitOfWork in _unitOfWorks)
        {
            if (unitOfWork is IChangeTrackingUnitOfWork tracked && !tracked.HasPendingChanges())
            {
                continue;
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        foreach (var source in _domainEventSources)
        {
            foreach (var aggregate in source.GetAggregateRootsWithPendingEvents())
            {
                foreach (var domainEvent in aggregate.DomainEvents.ToList())
                {
                    await _domainEventDispatcher.DispatchAsync(domainEvent, cancellationToken);
                }

                aggregate.ClearDomainEvents();
            }
        }

        return response;
    }
}
