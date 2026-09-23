using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorWeb;
using BlazorWeb.Services;
using Clients.Infrastructure.DependencyInjection;
using Clients.Infrastructure.Grooming;
using Clients.Infrastructure.Persistence;
using SharedUI.DependencyInjection;
using SQLitePCL;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7180/";
if (!apiBaseUrl.EndsWith('/'))
{
    apiBaseUrl += "/";
}

builder.Services.AddTransient<SharedUI.Http.AuthHandler>();
builder.Services.AddSingleton<SharedUI.Services.ITokenStorage, WebTokenStorage>();
builder.Services.AddSingleton<SharedUI.Services.IAuthState, SharedUI.Services.ClientAuthState>();
builder.Services.AddSingleton<SharedUI.Http.IAuthTokenRefresher, SharedUI.Http.AuthTokenRefresher>();

builder.Services.AddHttpClient("Auth", client => client.BaseAddress = new Uri(apiBaseUrl));

builder.Services.AddHttpClient("API", client => client.BaseAddress = new Uri(apiBaseUrl))
.AddHttpMessageHandler<SharedUI.Http.AuthHandler>();

builder.Services.AddHttpClient("PublicApi", client => client.BaseAddress = new Uri(apiBaseUrl));

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("API"));
builder.Services.AddScoped<Clients.Infrastructure.Billing.IClinicBillingApi>(sp =>
    new Clients.Infrastructure.Billing.ClinicBillingApiService(
        sp.GetRequiredService<IHttpClientFactory>().CreateClient("API")));
builder.Services.AddScoped<Clients.Infrastructure.Http.ApiClient>();
builder.Services.AddScoped<Clients.Infrastructure.Http.IWardUnitApiService, Clients.Infrastructure.Http.WardUnitApiService>();
builder.Services.AddScoped<Clients.Infrastructure.Crm.IClinicalAttachmentService, Clients.Infrastructure.Http.ClinicalAttachmentService>();
builder.Services.AddScoped<Clients.Infrastructure.Http.IPurchaseImportApiService, Clients.Infrastructure.Http.PurchaseImportApiService>();
builder.Services.AddScoped<Clients.Infrastructure.Http.IInventoryCountApiService, Clients.Infrastructure.Http.InventoryCountApiService>();
builder.Services.AddScoped<Clients.Infrastructure.Http.IProductLabelApiService, Clients.Infrastructure.Http.ProductLabelApiService>();
builder.Services.AddScoped<SharedUI.Services.IFileDownloadService, BlazorWeb.Services.WebFileDownloadService>();
builder.Services.AddSingleton<SharedUI.Services.IBarcodeScannerService, BlazorWeb.Services.WebBarcodeScannerService>();

builder.Services.AddSharedUI();
builder.Services.AddSingleton<IGroomingStatusRealtime>(sp =>
{
    var tokenStorage = sp.GetRequiredService<SharedUI.Services.ITokenStorage>();
    var connectivity = sp.GetRequiredService<SharedUI.Services.IConnectivityService>();
    return new GroomingStatusRealtimeService(
        () => sp.GetService<IHttpClientFactory>(),
        () => tokenStorage.GetAccessTokenAsync(),
        () => connectivity.IsOnline);
});
builder.Services.AddSingleton<SharedUI.Services.INavigationService, WebNavigationService>();
builder.Services.AddSingleton<SharedUI.Services.IConnectivityService, WebConnectivityService>();
builder.Services.AddSingleton<Clients.Infrastructure.Sync.ISyncConnectivity, SharedUI.Services.SyncConnectivityAdapter>();

builder.Services.AddScoped<SharedUI.Services.IVeterinaryApiService, SharedUI.Services.MockVeterinaryApiService>();
builder.Services.AddScoped<SharedUI.Services.IInventoryApiService, SharedUI.Services.MockInventoryApiService>();
Batteries_V2.Init();

builder.Services.AddSingleton<ISqliteFilePersistence, WebIndexedDbSqlitePersistence>();
builder.Services.AddClientPersistence($"Data Source={SqliteFileHelper.DatabaseFileName}");

builder.Services.AddHttpClient<Clients.Infrastructure.Sync.ISyncHttpClient, Clients.Infrastructure.Sync.SyncHttpClient>(client =>
    client.BaseAddress = new Uri(apiBaseUrl))
.AddHttpMessageHandler<SharedUI.Http.AuthHandler>();
builder.Services.AddHostedService<Clients.Infrastructure.Sync.SyncBackgroundWorker>();

var host = builder.Build();

await host.Services.RestoreOfflineDatabaseIfExistsAsync();
await host.Services.MigrateOfflineDatabaseAsync();

var authState = host.Services.GetRequiredService<SharedUI.Services.IAuthState>();
await authState.InitializeAsync();

var connectivityService = host.Services.GetRequiredService<SharedUI.Services.IConnectivityService>() as WebConnectivityService;
if (connectivityService != null)
{
    await connectivityService.InitializeAsync();
}

await host.RunAsync();
