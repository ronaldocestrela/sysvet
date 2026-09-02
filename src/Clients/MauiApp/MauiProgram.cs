using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.EntityFrameworkCore;

namespace MauiApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();
#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
#endif

		builder.Services.AddTransient<MauiApp.Services.AuthHandler>();

		builder.Services.AddHttpClient("API", client => 
		{
			// Replace with actual API url, using Android emulator localhost mapping for now
			client.BaseAddress = new Uri("http://10.0.2.2:7001/"); 
		})
		.AddHttpMessageHandler<MauiApp.Services.AuthHandler>();

		builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("API"));

		builder.Services.AddSingleton<SharedUI.Services.IAuthState, MauiApp.Services.MauiAuthState>();
		builder.Services.AddSingleton<SharedUI.Services.INavigationService, MauiApp.Services.MauiNavigationService>();
		builder.Services.AddScoped<SharedUI.Services.IConnectivityService, MauiApp.Services.MauiConnectivityService>();

		// Módulo Veterinary
		builder.Services.AddScoped<SharedUI.Services.IVeterinaryApiService, SharedUI.Services.MockVeterinaryApiService>();

		// SQLite Offline DB
		var dbPath = Path.Combine(FileSystem.AppDataDirectory, "sysvet.db");
		builder.Services.AddDbContext<Clients.Infrastructure.OfflineDbContext>(options =>
		{
			options.UseSqlite($"Data Source={dbPath}");
		});
		builder.Services.AddScoped(typeof(Clients.Infrastructure.IOfflineRepository<>), typeof(Clients.Infrastructure.OfflineRepository<>));

		// Sync Engine
		builder.Services.AddHttpClient<Clients.Infrastructure.Sync.ISyncHttpClient, Clients.Infrastructure.Sync.SyncHttpClient>(client =>
		{
			client.BaseAddress = new Uri("http://10.0.2.2:7001/"); 
		}).AddHttpMessageHandler<MauiApp.Services.AuthHandler>();
		builder.Services.AddHostedService<Clients.Infrastructure.Sync.SyncBackgroundWorker>();

		return builder.Build();
	}
}
