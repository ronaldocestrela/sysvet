using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SharedUI.DependencyInjection;
using SharedUI.Http;
using SharedUI.Services;
using TutorPortalWeb;
using TutorPortalWeb.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7180/";
if (!apiBaseUrl.EndsWith('/'))
{
    apiBaseUrl += "/";
}

builder.Services.AddTransient<AuthHandler>();
builder.Services.AddSingleton<ITokenStorage, WebTokenStorage>();
builder.Services.AddSingleton<IAuthState, ClientAuthState>();
builder.Services.AddSingleton<IAuthTokenRefresher, TutorAuthTokenRefresher>();
builder.Services.AddSingleton<INavigationService, WebNavigationService>();

builder.Services.AddHttpClient("Auth", client => client.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient("API", client => client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<AuthHandler>();
builder.Services.AddHttpClient("PublicApi", client => client.BaseAddress = new Uri(apiBaseUrl));

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("API"));
builder.Services.AddScoped<Clients.Infrastructure.Http.ApiClient>();
builder.Services.AddScoped<Clients.Infrastructure.TutorPortal.TutorPortalApiService>();

builder.Services.AddSharedUI();

var host = builder.Build();
var authState = host.Services.GetRequiredService<IAuthState>();
await authState.InitializeAsync();
await host.RunAsync();
