using API.Extensions;
using Core.Infrastructure.Identity;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHealthChecks();
builder.Services.AddProblemDetails();
builder.Services.AddApiDocumentation();
builder.Services.AddCoreModule(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
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

// Tratamento de exceções padrão do .NET para mapear ProblemDetails
app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseMiddleware<TenantClaimMiddleware>();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapCoreEndpoints();
app.MapAuthEndpoints();

// Hello world route for testing PoC
app.MapGet("/", () => "SysVet API is running!");

app.Run();

public partial class Program { }
