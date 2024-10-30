using Microsoft.Extensions.Configuration;

namespace DarkPatterns.ProxyDevelopmentServer.Demo;

public class Startup
{
	public void ConfigureServices(IServiceCollection services)
	{
		services.AddSpaStaticFiles(configuration =>
		{
			configuration.RootPath = "../ProxyDevelopmentServer.Demo.React/dist";
		});
	}

	public void Configure(IApplicationBuilder app)
	{
		// Keep stray POSTs from hitting the SPA middleware
		// Based on a comment in https://github.com/dotnet/aspnetcore/issues/5192
		app.MapWhen(context => context.Request.Method == "GET" || context.Request.Method == "CONNECT", (when) =>
		{
			var options = app.ApplicationServices.GetService<ProxyDevelopmentServerOptions>();
			app.UseSpaStaticFiles();
			app.UseSpa(spa =>
			{
				spa.Options.SourcePath = "../ProxyDevelopmentServer.Demo.React";

				spa.UseProxyDevelopmentServer(options ?? ProxyDevelopmentServerOptions.PnpmViteServer);
			});
		});
	}
}
