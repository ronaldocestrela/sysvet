using API.Extensions;
using Core.Domain;

namespace API.Middlewares;

/// <summary>
/// Converts handler return values of type <see cref="Result"/> or <see cref="Result{TValue}"/> into HTTP responses.
/// Handlers that already return <see cref="IResult"/> are left unchanged.
/// </summary>
public sealed class ResultEndpointFilter : IEndpointFilter
{
    /// <inheritdoc />
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var result = await UnwrapAsync(await next(context));
        return MapToHttpResult(result);
    }

    private static async ValueTask<object?> UnwrapAsync(object? value)
    {
        if (value is not Task task)
        {
            return value;
        }

        await task.ConfigureAwait(false);
        var taskType = task.GetType();
        return taskType.IsGenericType
            ? taskType.GetProperty(nameof(Task<object>.Result))!.GetValue(task)
            : null;
    }

    private static object? MapToHttpResult(object? result)
    {
        if (result is null or IResult)
        {
            return result;
        }

        var resultType = result.GetType();
        if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var isSuccess = (bool)resultType.GetProperty(nameof(Result.IsSuccess))!.GetValue(result)!;
            if (!isSuccess)
            {
                return ((Result)result).ToHttpResult();
            }

            var value = resultType.GetProperty(nameof(Result<object>.Value))!.GetValue(result);
            return Results.Ok(value);
        }

        if (result is Result domainResult)
        {
            return domainResult.ToHttpResult();
        }

        return result;
    }
}
