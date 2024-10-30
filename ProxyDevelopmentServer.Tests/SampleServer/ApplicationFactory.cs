using DarkPatterns.ProxyDevelopmentServer;
using DarkPatterns.ProxyDevelopmentServer.Demo;

using Microsoft.AspNetCore.Mvc.Testing;

namespace DarkPatterns.SampleServer;

public class ApplicationFactory : WebApplicationFactory<Startup>
{
	public ApplicationFactory()
	{
		// Ensure the working directory is set to the Demo directory; this is where the app expects to run under normal debugging
		Directory.SetCurrentDirectory(
			Path.GetFullPath(
				"ProxyDevelopmentServer.Demo",
				SolutionUtils.GetSolutionDirectory(Directory.GetCurrentDirectory(), "DarkPatterns.ProxyDevelopmentServer.sln")
			)
		);
	}
}
