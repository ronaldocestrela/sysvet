using System.Net;
using Core.Domain;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using API.Middlewares;

namespace API.IntegrationTests;

public class ResultEndpointFilterTests
{
    private static DefaultHttpContext CreateHttpContext()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddProblemDetails();
        return new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider()
        };
    }

    [Fact]
    public async Task Filter_ConvertsFailureResult_To404ProblemDetails()
    {
        var filter = new ResultEndpointFilter();
        var context = CreateHttpContext();
        var invocation = EndpointFilterInvocationContext.Create(context);

        var output = await filter.InvokeAsync(
            invocation,
            _ => ValueTask.FromResult<object?>(Result.Failure(ErrorCodes.Pet.NotFound)));

        var httpResult = output.Should().BeAssignableTo<IResult>().Subject;
        context.Response.Body = new MemoryStream();
        await httpResult.ExecuteAsync(context);
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Filter_ConvertsSuccessResult_To200WithBody()
    {
        var filter = new ResultEndpointFilter();
        var context = CreateHttpContext();
        var invocation = EndpointFilterInvocationContext.Create(context);

        var output = await filter.InvokeAsync(
            invocation,
            _ => ValueTask.FromResult<object?>(Result.Success("ok")));

        var httpResult = output.Should().BeAssignableTo<IResult>().Subject;
        context.Response.Body = new MemoryStream();
        await httpResult.ExecuteAsync(context);
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.OK);
    }

    [Fact]
    public async Task Filter_LeavesIResult_Unchanged()
    {
        var filter = new ResultEndpointFilter();
        var context = new DefaultHttpContext();
        var invocation = EndpointFilterInvocationContext.Create(context);
        var original = Results.Ok("direct");

        var output = await filter.InvokeAsync(
            invocation,
            _ => ValueTask.FromResult<object?>(original));

        output.Should().BeSameAs(original);
    }
}
