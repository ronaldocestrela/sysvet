using System;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace MauiApp;

class Program : MauiApplication
{
	protected override Microsoft.Maui.Hosting.MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

	static void Main(string[] args)
	{
		var app = new Program();
		app.Run(args);
	}
}
