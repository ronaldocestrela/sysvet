using ClinicSiteWeb;
using ClinicSiteWeb.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SharedUI.DependencyInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7180/";
if (!apiBaseUrl.EndsWith('/'))
{
    apiBaseUrl += "/";
}

builder.Services.AddSingleton<SlugContext>();
builder.Services.AddScoped<PublicClinicSiteApiService>();
builder.Services.AddScoped<PublicStoreApiService>();
builder.Services.AddHttpClient("PublicApi", client => client.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddSharedUI();

await builder.Build().RunAsync();
