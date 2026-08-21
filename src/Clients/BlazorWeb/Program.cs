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

var host = builder.Build();

// Inicializa o serviço de conectividade para registrar os listeners JS
var connectivityService = host.Services.GetRequiredService<SharedUI.Services.IConnectivityService>() as BlazorWeb.Services.WebConnectivityService;
if (connectivityService != null)
{
    await connectivityService.InitializeAsync();
}

await host.RunAsync();
