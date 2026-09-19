using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.Configuration;
using Clients.Infrastructure.DependencyInjection;
using SharedUI.DependencyInjection;
using SharedUI.Http;
using SharedUI.Services;

namespace MauiApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder.UseMauiApp<App>();
		TryAddAppSettings(builder);

		builder.Services.AddMauiBlazorWebView();
#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
#endif

		var apiBaseUrl = MauiApiConfiguration.ResolveApiBaseUrl(builder.Configuration);

		builder.Services.AddTransient<AuthHandler>();
		builder.Services.AddSingleton<ITokenStorage, MauiApp.Services.MauiSecureTokenStorage>();
		builder.Services.AddSingleton<IAuthState, ClientAuthState>();
		builder.Services.AddSingleton<IAuthTokenRefresher, AuthTokenRefresher>();

		builder.Services.AddHttpClient("Auth", client => client.BaseAddress = new Uri(apiBaseUrl));

		builder.Services.AddHttpClient("API", client => client.BaseAddress = new Uri(apiBaseUrl))
			.AddHttpMessageHandler<AuthHandler>();

		builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("API"));
		builder.Services.AddScoped<Clients.Infrastructure.Http.ApiClient>();
		builder.Services.AddScoped<Clients.Infrastructure.Http.IWardUnitApiService, Clients.Infrastructure.Http.WardUnitApiService>();
		builder.Services.AddScoped<Clients.Infrastructure.Crm.IClinicalAttachmentService, Clients.Infrastructure.Http.ClinicalAttachmentService>();
		builder.Services.AddScoped<Clients.Infrastructure.Http.IPurchaseImportApiService, Clients.Infrastructure.Http.PurchaseImportApiService>();

		builder.Services.AddSharedUI();
		builder.Services.AddSingleton<INavigationService, MauiApp.Services.MauiNavigationService>();
		builder.Services.AddSingleton<IConnectivityService, MauiApp.Services.MauiConnectivityService>();
		builder.Services.AddSingleton<Clients.Infrastructure.Sync.ISyncConnectivity, SyncConnectivityAdapter>();

		builder.Services.AddScoped<IVeterinaryApiService, MockVeterinaryApiService>();
		builder.Services.AddScoped<IInventoryApiService, MockInventoryApiService>();
		builder.Services.AddScoped<ISalesApiService, MockSalesApiService>();

		var dbPath = Path.Combine(FileSystem.AppDataDirectory, "sysvet.db");
		builder.Services.AddClientPersistence($"Data Source={dbPath}");

		builder.Services.AddHttpClient<Clients.Infrastructure.Sync.ISyncHttpClient, Clients.Infrastructure.Sync.SyncHttpClient>(client =>
			client.BaseAddress = new Uri(apiBaseUrl))
			.AddHttpMessageHandler<AuthHandler>();
		builder.Services.AddHostedService<Clients.Infrastructure.Sync.SyncBackgroundWorker>();

		var app = builder.Build();

		app.Services.MigrateOfflineDatabaseAsync().GetAwaiter().GetResult();

		var authState = app.Services.GetRequiredService<IAuthState>();
		authState.InitializeAsync().GetAwaiter().GetResult();

		return app;
	}

	private static void TryAddAppSettings(MauiAppBuilder builder)
	{
		try
		{
			using var stream = FileSystem.OpenAppPackageFileAsync("appsettings.json").GetAwaiter().GetResult();
			builder.Configuration.AddJsonStream(stream);
		}
		catch (FileNotFoundException)
		{
			// Optional override; platform defaults apply via MauiApiConfiguration.
		}
	}
}
