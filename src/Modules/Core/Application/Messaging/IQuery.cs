using Core.Domain;
using MediatR;

namespace Core.Application.Messaging;

/// <summary>
/// Read-only request that returns data wrapped in <see cref="Result{TValue}"/>.
/// </summary>
/// <typeparam name="TResponse">Query result type inside the result.</typeparam>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
