using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.EntityFrameworkCore;
using BlazorWeb;
using SharedUI.DependencyInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7180/";
if (!apiBaseUrl.EndsWith('/'))
{
    apiBaseUrl += "/";
}

builder.Services.AddTransient<SharedUI.Http.AuthHandler>();
builder.Services.AddSingleton<SharedUI.Services.ITokenStorage, BlazorWeb.Services.WebTokenStorage>();
builder.Services.AddSingleton<SharedUI.Services.IAuthState, SharedUI.Services.ClientAuthState>();
builder.Services.AddSingleton<SharedUI.Http.IAuthTokenRefresher, SharedUI.Http.AuthTokenRefresher>();

builder.Services.AddHttpClient("Auth", client => client.BaseAddress = new Uri(apiBaseUrl));

builder.Services.AddHttpClient("API", client => client.BaseAddress = new Uri(apiBaseUrl))
.AddHttpMessageHandler<SharedUI.Http.AuthHandler>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("API"));
builder.Services.AddScoped<Clients.Infrastructure.Http.ApiClient>();

builder.Services.AddSharedUI();
builder.Services.AddSingleton<SharedUI.Services.INavigationService, BlazorWeb.Services.WebNavigationService>();
builder.Services.AddScoped<SharedUI.Services.IConnectivityService, BlazorWeb.Services.WebConnectivityService>();

// Módulo Veterinary
builder.Services.AddScoped<SharedUI.Services.IVeterinaryApiService, SharedUI.Services.MockVeterinaryApiService>();

// Módulo Inventory
builder.Services.AddScoped<SharedUI.Services.IInventoryApiService, SharedUI.Services.MockInventoryApiService>();
builder.Services.AddScoped<SharedUI.Services.ISalesApiService, SharedUI.Services.MockSalesApiService>();

// SQLite Offline DB
builder.Services.AddDbContext<Clients.Infrastructure.OfflineDbContext>(options =>
{
    // Em WASM local storage (Origin Private File System)
    options.UseSqlite("Data Source=sysvet.db");
});
builder.Services.AddScoped(typeof(Clients.Infrastructure.IOfflineRepository<>), typeof(Clients.Infrastructure.OfflineRepository<>));

// Sync Engine
builder.Services.AddHttpClient<Clients.Infrastructure.Sync.ISyncHttpClient, Clients.Infrastructure.Sync.SyncHttpClient>(client =>
    client.BaseAddress = new Uri(apiBaseUrl))
.AddHttpMessageHandler<SharedUI.Http.AuthHandler>();
// Nota: Em Blazor WASM, HostedServices podem não rodar em background da mesma forma que MAUI.
// Requer .NET 8+ com suporte nativo ou inicialização manual em background.
builder.Services.AddHostedService<Clients.Infrastructure.Sync.SyncBackgroundWorker>();

var host = builder.Build();

var authState = host.Services.GetRequiredService<SharedUI.Services.IAuthState>();
await authState.InitializeAsync();

// Inicializa o serviço de conectividade para registrar os listeners JS
var connectivityService = host.Services.GetRequiredService<SharedUI.Services.IConnectivityService>() as BlazorWeb.Services.WebConnectivityService;
if (connectivityService != null)
{
    await connectivityService.InitializeAsync();
}

await host.RunAsync();
