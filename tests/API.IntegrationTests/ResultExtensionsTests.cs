using System.Net;
using Core.Domain;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using API.Extensions;

namespace API.IntegrationTests;

public class ResultExtensionsTests
{
    private static async Task<(int StatusCode, string Body)> ExecuteAsync(IResult result)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddProblemDetails();
        var context = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider(),
            TraceIdentifier = "test-correlation-id"
        };
        context.Response.Body = new MemoryStream();
        await result.ExecuteAsync(context);
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var body = await reader.ReadToEndAsync();
        return (context.Response.StatusCode, body);
    }

    [Fact]
    public void ToProblemDetails_OnSuccess_ThrowsInvalidOperationException()
    {
        var result = Result.Success();
        var act = () => result.ToProblemDetails();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public async Task ToProblemDetails_NotFound_Returns404()
    {
        var result = Result.Failure(ErrorCodes.Pet.NotFound);
        var (status, _) = await ExecuteAsync(result.ToProblemDetails());
        status.Should().Be((int)HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ToProblemDetails_Conflict_Returns409()
    {
        var result = Result.Failure(new Error("Product.Conflict", "Duplicate barcode."));
        var (status, _) = await ExecuteAsync(result.ToProblemDetails());
        status.Should().Be((int)HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task ToProblemDetails_Unauthorized_Returns401()
    {
        var result = Result.Failure(new Error("Auth.Unauthorized", "Invalid credentials."));
        var (status, _) = await ExecuteAsync(result.ToProblemDetails());
        status.Should().Be((int)HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ToProblemDetails_Forbidden_Returns403()
    {
        var result = Result.Failure(new Error("Access.Forbidden", "Not allowed."));
        var (status, _) = await ExecuteAsync(result.ToProblemDetails());
        status.Should().Be((int)HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ToProblemDetails_Validation_Returns400()
    {
        var validation = ValidationResult.WithErrors([new Error("Name", "Required")]);
        var (status, _) = await ExecuteAsync(validation.ToProblemDetails());
        status.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ToHttpResult_SuccessWithoutValue_Returns204()
    {
        var result = Result.Success();
        var (status, _) = await ExecuteAsync(result.ToHttpResult());
        status.Should().Be((int)HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ToProblemDetails_RouteIdMismatch_IncludesErrorCode()
    {
        var context = new DefaultHttpContext { TraceIdentifier = "corr-123" };
        var (status, body) = await ExecuteAsync(ApiResultHelpers.RouteIdMismatch(context));
        status.Should().Be((int)HttpStatusCode.BadRequest);
        body.Should().Contain("Request.RouteIdMismatch");
        body.Should().Contain("corr-123");
    }

    [Fact]
    public async Task ToHttpResult_SuccessWithValue_Returns200()
    {
        var result = Result.Success(42);
        var (status, _) = await ExecuteAsync(result.ToHttpResult());
        status.Should().Be((int)HttpStatusCode.OK);
    }

    [Fact]
    public async Task ToCreatedAt_Success_Returns201()
    {
        var result = Result.Success(Guid.NewGuid());
        var (status, _) = await ExecuteAsync(result.ToCreatedAt("/api/v1/tutors/1"));
        status.Should().Be((int)HttpStatusCode.Created);
    }
}
