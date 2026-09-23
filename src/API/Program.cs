using API.Extensions;
using API.Hubs;
using API.Filters;
using API.Middlewares;
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
app.UseMiddleware<PartnerApiKeyMiddleware>();
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseMiddleware<ImpersonationSessionMiddleware>();
app.UseAuthorization();
app.UseMiddleware<TenantRequestMetricsMiddleware>();

app.MapApiHealthChecks();

var routes = app.MapGroup(string.Empty)
    .AddEndpointFilter<ResultEndpointFilter>()
    .AddEndpointFilter<TenantRequiredEndpointFilter>()
    .AddEndpointFilter<TenantOperationalBillingEndpointFilter>()
    .AddEndpointFilter<CommercialModuleEndpointFilter>();

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
routes.MapCommerceEndpoints();
routes.MapCommercePublicEndpoints();
routes.MapPlatformEndpoints();
routes.MapPartnerEndpoints();
routes.MapClinicBillingEndpoints();
routes.MapPlatformBillingWebhookEndpoints();

app.MapHub<GroomingStatusHub>(GroomingStatusHub.HubPath);

app.MapGet("/", () => "SysVet API is running!");

app.Run();

public partial class Program { }
