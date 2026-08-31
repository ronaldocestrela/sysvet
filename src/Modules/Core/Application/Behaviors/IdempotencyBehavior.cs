using Core.Application.Common;
using Core.Application.Common.Interfaces;
using Core.Domain;
using MediatR;

namespace Core.Application.Behaviors;

public class IdempotencyBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : class
{
    private readonly IIdempotencyService _idempotencyService;

    public IdempotencyBehavior(IIdempotencyService idempotencyService)
    {
        _idempotencyService = idempotencyService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not IIdempotentCommand<TResponse> idempotentCommand)
        {
            return await next();
        }

        var key = idempotentCommand.IdempotencyKey;

        if (key == Guid.Empty)
        {
            return await next();
        }

        bool exists = await _idempotencyService.RequestExistsAsync(key, cancellationToken);

        if (exists)
        {
            var type = typeof(TResponse);

            if (type == typeof(Result))
            {
                return (Result.Success() as TResponse)!;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Result<>))
            {
                var genericType = type.GetGenericArguments()[0];
                var successMethod = typeof(Result).GetMethods().First(m => m.Name == "Success" && m.IsGenericMethod);
                var genericSuccessMethod = successMethod.MakeGenericMethod(genericType);
                
                var defaultValue = genericType.IsValueType ? Activator.CreateInstance(genericType) : null;
                
                return (TResponse)genericSuccessMethod.Invoke(null, new[] { defaultValue })!;
            }

            return default!;
        }

        var response = await next();

        bool isSuccess = true;
        if (response is Result result)
        {
            isSuccess = result.IsSuccess;
        }

        if (isSuccess)
        {
            await _idempotencyService.CreateRequestAsync(key, typeof(TRequest).Name, cancellationToken);
        }

        return response;
    }
}
