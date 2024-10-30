using DarkPatterns.ProxyDevelopmentServer.Demo;

Host.CreateDefaultBuilder(args)
	.ConfigureWebHostDefaults(webBuilder =>
	{
		webBuilder.UseStartup<Startup>();
	})
	.Build()
	.Run();
