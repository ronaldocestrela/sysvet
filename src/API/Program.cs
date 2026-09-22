using API.Extensions;
using API.Hubs;
using API.Middlewares;
using Core.Infrastructure.Identity;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = ctx =>
    {
        ctx.ProblemDetails.Extensions["correlationId"] = ctx.HttpContext.TraceIdentifier;
    };
});
builder.Services.AddApiDocumentation();
builder.Services.AddApplicationModules(builder.Configuration);
builder.Services.AddApiHealthChecks();
builder.Services.AddBlazorWebCors(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("SysVet API")
               .WithTheme(ScalarTheme.DeepSpace)
               .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseHttpsRedirection();
app.UseBlazorWebCors();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseAuthentication();
app.UseMiddleware<TenantClaimMiddleware>();
app.UseAuthorization();

app.MapApiHealthChecks();

var routes = app.MapGroup(string.Empty)
    .AddEndpointFilter<ResultEndpointFilter>();

routes.MapVeterinaryEndpoints();
routes.MapCoreEndpoints();
routes.MapAuthEndpoints(app.Environment);
routes.MapInventoryEndpoints();
routes.MapSalesEndpoints();
routes.MapPetshopEndpoints();
routes.MapFinanceEndpoints();
routes.MapFiscalEndpoints();
routes.MapAutomationsEndpoints();
routes.MapAutomationsPublicNpsEndpoints();
routes.MapTutorPortalEndpoints();
routes.MapClinicSiteEndpoints();
routes.MapClinicSitePublicEndpoints();

app.MapHub<GroomingStatusHub>(GroomingStatusHub.HubPath);

app.MapGet("/", () => "SysVet API is running!");

app.Run();

public partial class Program { }
