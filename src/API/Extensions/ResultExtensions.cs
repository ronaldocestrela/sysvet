using Core.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Extensions;

public static class ResultExtensions
{
    public static IResult ToProblemDetails(this Result result)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("Não é possível criar ProblemDetails de um resultado de sucesso.");
        }

        if (result is IValidationResult validationResult)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Ocorreram um ou mais erros de validação.",
                type: "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                detail: "Verifique a propriedade 'errors' para mais detalhes.",
                extensions: new Dictionary<string, object?>
                {
                    { "errors", validationResult.ValidationErrors }
                }
            );
        }

        return Results.Problem(
            statusCode: GetStatusCode(result.Error.Code),
            title: "Ocorreu um erro ao processar a requisição.",
            type: "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            detail: result.Error.Message,
            extensions: new Dictionary<string, object?>
            {
                { "errors", new[] { new { Code = result.Error.Code, Message = result.Error.Message } } }
            }
        );
    }

    private static int GetStatusCode(string errorCode) =>
        errorCode switch
        {
            var code when code.EndsWith("NotFound") => StatusCodes.Status404NotFound,
            var code when code.Contains("Unauthorized") => StatusCodes.Status401Unauthorized,
            var code when code.Contains("Forbidden") => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status400BadRequest
        };
}
