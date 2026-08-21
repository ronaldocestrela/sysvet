using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.EntityFrameworkCore;
using BlazorWeb;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddTransient<BlazorWeb.Services.AuthHandler>();

builder.Services.AddHttpClient("API", client => 
{
    client.BaseAddress = new Uri("https://localhost:7001/"); // Replace with actual API url later
})
.AddHttpMessageHandler<BlazorWeb.Services.AuthHandler>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("API"));

builder.Services.AddSingleton<SharedUI.Services.IAuthState, BlazorWeb.Services.WebAuthState>();
builder.Services.AddSingleton<SharedUI.Services.INavigationService, BlazorWeb.Services.WebNavigationService>();
builder.Services.AddScoped<SharedUI.Services.IConnectivityService, BlazorWeb.Services.WebConnectivityService>();

// SQLite Offline DB
builder.Services.AddDbContext<Clients.Infrastructure.OfflineDbContext>(options =>
{
    // Em WASM local storage (Origin Private File System)
    options.UseSqlite("Data Source=sysvet.db");
});
builder.Services.AddScoped(typeof(Clients.Infrastructure.IOfflineRepository<>), typeof(Clients.Infrastructure.OfflineRepository<>));

// Sync Engine
builder.Services.AddHttpClient<Clients.Infrastructure.Sync.ISyncHttpClient, Clients.Infrastructure.Sync.SyncHttpClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7001/"); 
}).AddHttpMessageHandler<BlazorWeb.Services.AuthHandler>();
// Nota: Em Blazor WASM, HostedServices podem não rodar em background da mesma forma que MAUI.
// Requer .NET 8+ com suporte nativo ou inicialização manual em background.
builder.Services.AddHostedService<Clients.Infrastructure.Sync.SyncBackgroundWorker>();

var host = builder.Build();

// Inicializa o serviço de conectividade para registrar os listeners JS
var connectivityService = host.Services.GetRequiredService<SharedUI.Services.IConnectivityService>() as BlazorWeb.Services.WebConnectivityService;
if (connectivityService != null)
{
    await connectivityService.InitializeAsync();
}

await host.RunAsync();
