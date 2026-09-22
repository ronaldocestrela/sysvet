using Core.Domain;
using Microsoft.AspNetCore.Http;

namespace API.Extensions;

/// <summary>
/// Maps domain <see cref="Result"/> values to ASP.NET Core minimal API HTTP results.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Maps a failed result to RFC 7807 Problem Details with a status derived from <see cref="Error.Code"/>.
    /// </summary>
    public static IResult ToProblemDetails(this Result result) =>
        result.ToProblemDetails(httpContext: null);

    /// <summary>
    /// Maps a failed result to RFC 7807 Problem Details including correlation id when HTTP context is available.
    /// </summary>
    public static IResult ToProblemDetails(this Result result, HttpContext? httpContext)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("Não é possível criar ProblemDetails de um resultado de sucesso.");
        }

        if (result is IValidationResult validationResult)
        {
            var validationExtensions = new Dictionary<string, object?>
            {
                { "errors", validationResult.ValidationErrors }
            };
            AppendCorrelationId(httpContext, validationExtensions);

            return Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Ocorreram um ou mais erros de validação.",
                type: "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                detail: "Verifique a propriedade 'errors' para mais detalhes.",
                extensions: validationExtensions);
        }

        var extensions = BuildFailureExtensions(result.Error);
        AppendCorrelationId(httpContext, extensions);

        return Results.Problem(
            statusCode: GetStatusCode(result.Error.Code),
            title: "Ocorreu um erro ao processar a requisição.",
            type: "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            detail: result.Error.Message,
            extensions: extensions);
    }

    /// <summary>
    /// Maps any result to an HTTP response: success without payload as 204, success with payload as 200, failure as Problem Details.
    /// </summary>
    public static IResult ToHttpResult(this Result result, HttpContext? httpContext = null)
    {
        if (result.IsFailure)
        {
            return result.ToProblemDetails(httpContext);
        }

        return Results.NoContent();
    }

    /// <summary>
    /// Maps a typed result to an HTTP response: success as 200 with body, failure as Problem Details.
    /// </summary>
    public static IResult ToHttpResult<T>(this Result<T> result, HttpContext? httpContext = null) =>
        result.IsSuccess ? Results.Ok(result.Value) : result.ToProblemDetails(httpContext);

    /// <summary>
    /// Maps a successful typed result to 201 Created at the given location; failures use Problem Details.
    /// </summary>
    public static IResult ToCreatedAt<T>(this Result<T> result, string location, HttpContext? httpContext = null) =>
        result.IsSuccess ? Results.Created(location, result.Value) : result.ToProblemDetails(httpContext);

    private static Dictionary<string, object?> BuildFailureExtensions(Error error) =>
        new()
        {
            { "errors", new[] { new { Code = error.Code, Message = error.Message } } }
        };

    private static void AppendCorrelationId(HttpContext? httpContext, Dictionary<string, object?> extensions)
    {
        if (httpContext is not null && !string.IsNullOrWhiteSpace(httpContext.TraceIdentifier))
        {
            extensions["correlationId"] = httpContext.TraceIdentifier;
        }
    }

    private static int GetStatusCode(string errorCode) =>
        errorCode switch
        {
            var code when code.EndsWith("NotFound") => StatusCodes.Status404NotFound,
            var code when code.Contains("Conflict") => StatusCodes.Status409Conflict,
            var code when code.Contains("InvalidCredentials") => StatusCodes.Status401Unauthorized,
            var code when code.Contains("TenantNotActive") => StatusCodes.Status401Unauthorized,
            var code when code.Contains("InvalidRefreshToken") => StatusCodes.Status401Unauthorized,
            var code when code.Contains("WrongPortal") => StatusCodes.Status401Unauthorized,
            var code when code.Contains("Unauthorized") => StatusCodes.Status401Unauthorized,
            var code when code.Contains("RegistrationDenied") => StatusCodes.Status403Forbidden,
            var code when code.Contains("Forbidden") => StatusCodes.Status403Forbidden,
            var code when code.Contains("LockedOut") => StatusCodes.Status403Forbidden,
            var code when code.Contains("DuplicateEmail") => StatusCodes.Status409Conflict,
            var code when code.Contains("DuplicateCpf") => StatusCodes.Status409Conflict,
            var code when code.Contains("RegistrationNotAllowed") => StatusCodes.Status403Forbidden,
            var code when code.Contains("Disabled") => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status400BadRequest
        };
}
