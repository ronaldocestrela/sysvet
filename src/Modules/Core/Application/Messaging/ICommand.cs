using Core.Domain;
using MediatR;

namespace Core.Application.Messaging;

/// <summary>
/// Marker for command requests that mutate state and participate in the transactional pipeline.
/// </summary>
public interface ICommandBase { }

/// <summary>
/// Command that returns a typed payload wrapped in <see cref="Result{TValue}"/>.
/// </summary>
/// <typeparam name="TResponse">Success payload type inside the result.</typeparam>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>, ICommandBase
{
}

/// <summary>
/// Command with no success payload beyond success/failure.
/// </summary>
public interface ICommand : IRequest<Result>, ICommandBase
{
}
