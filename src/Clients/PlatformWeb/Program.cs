using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Clients.Infrastructure.Http;
using Clients.Infrastructure.Platform;
using PlatformWeb;
using PlatformWeb.Services;
using SharedUI.DependencyInjection;
using SharedUI.Http;
using SharedUI.Services;

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
builder.Services.AddSingleton<IPlatformAuthState, PlatformAuthState>();
builder.Services.AddSingleton<IAuthState>(sp => sp.GetRequiredService<IPlatformAuthState>());
builder.Services.AddSingleton<IAuthTokenRefresher, AuthTokenRefresher>();
builder.Services.AddSingleton<INavigationService, WebNavigationService>();

builder.Services.AddHttpClient("Auth", client => client.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient("API", client => client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<AuthHandler>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("API"));
builder.Services.AddScoped<ApiClient>();
builder.Services.AddScoped<IPlatformAdminApi, PlatformAdminApiService>();
builder.Services.AddScoped<IFileDownloadService, WebFileDownloadService>();

builder.Services.AddSharedUI();

var host = builder.Build();
var authState = host.Services.GetRequiredService<IAuthState>();
await authState.InitializeAsync();
await host.RunAsync();
