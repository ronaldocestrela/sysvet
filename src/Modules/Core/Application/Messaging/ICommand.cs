using Core.Domain;
using MediatR;

namespace Core.Application.Messaging;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}

public interface ICommand : IRequest<Result>
{
}
