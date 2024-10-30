using Microsoft.Extensions.Configuration;

namespace DarkPatterns.ProxyDevelopmentServer.Demo;

public class Startup
{
	private readonly IWebHostEnvironment env;

	public Startup(IWebHostEnvironment env)
	{
		this.env = env;
	}


	public void ConfigureServices(IServiceCollection services)
	{
		services.AddSpaStaticFiles(configuration =>
		{
			configuration.RootPath = "ui/dist";
		});
	}

	public void Configure(IApplicationBuilder app)
	{
		// Keep stray POSTs from hitting the SPA middleware
		// Based on a comment in https://github.com/dotnet/aspnetcore/issues/5192
		app.MapWhen(context => context.Request.Method == "GET" || context.Request.Method == "CONNECT", (when) =>
		{
			app.UseSpaStaticFiles();
			app.UseSpa(spa =>
			{
				spa.Options.SourcePath = "ui";

				spa.UseProxyDevelopmentServer("node_modules/.bin/vite", "--port {port}");
			});
		});
	}
}
