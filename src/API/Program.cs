using API.Extensions;
using API.Middlewares;
using Core.Infrastructure.Identity;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();
builder.Services.AddProblemDetails();
builder.Services.AddApiDocumentation();
builder.Services.AddApplicationModules(builder.Configuration);

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

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseAuthentication();
app.UseMiddleware<TenantClaimMiddleware>();
app.UseAuthorization();

app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false
});

app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => true
});

var routes = app.MapGroup(string.Empty)
    .AddEndpointFilter<ResultEndpointFilter>();

routes.MapCoreEndpoints();
routes.MapAuthEndpoints();
routes.MapVeterinaryEndpoints();
routes.MapInventoryEndpoints();
routes.MapSalesEndpoints();
routes.MapPetshopEndpoints();
routes.MapFiscalEndpoints();

app.MapGet("/", () => "SysVet API is running!");

app.Run();

public partial class Program { }
