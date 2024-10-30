using DarkPatterns.SampleServer;

using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace DarkPatterns.ProxyDevelopmentServer;

public class ProxyDevelopmentServerShould(ApplicationFactory factory)
		: IClassFixture<ApplicationFactory>
{
	[Theory]
	[InlineData("/", "text/html")]
	[InlineData("/Index", "text/html")]
	[InlineData("/About", "text/html")]
	[InlineData("/Privacy", "text/html")]
	[InlineData("/Contact", "text/html")]
	// The following file is for dev mode only; Vite should transpile the file on the fly
	[InlineData("/src/main.tsx", "text/javascript")]
	public Task Get_endpoints_return_success_and_correct_content_type_from_bin(string path, string contentType)
	{
		return VerifyEndpoints(path, contentType, ProxyDevelopmentServerOptions.DefaultViteServer);
	}

	[Theory]
	[InlineData("/", "text/html")]
	[InlineData("/Index", "text/html")]
	[InlineData("/About", "text/html")]
	[InlineData("/Privacy", "text/html")]
	[InlineData("/Contact", "text/html")]
	// The following file is for dev mode only; Vite should transpile the file on the fly
	[InlineData("/src/main.tsx", "text/javascript")]
	public Task Get_endpoints_return_success_and_correct_content_type_from_pnpm(string path, string contentType)
	{
		return VerifyEndpoints(path, contentType, ProxyDevelopmentServerOptions.PnpmViteServer);
	}

	private async Task VerifyEndpoints(string path, string contentType, ProxyDevelopmentServerOptions options)
	{
		// Arrange
		var client = factory
			.WithWebHostBuilder(builder =>
			{
				builder.ConfigureServices(services => services.AddSingleton(options));
				builder.UseSolutionRelativeContentRoot("ProxyDevelopmentServer.Demo");
			})
			.CreateClient();

		// Act
		var response = await client.GetAsync(path);

		// Assert
		response.EnsureSuccessStatusCode(); // Status Code 200-299
		Assert.Equal(contentType, response.Content.Headers.ContentType?.MediaType);
	}

	public static TheoryData<string, string, ProxyDevelopmentServerOptions?> GetOptions()
	{
		var result = new TheoryData<string, string, ProxyDevelopmentServerOptions?>();
		foreach (var serverOptions in new ProxyDevelopmentServerOptions?[]
		{
			ProxyDevelopmentServerOptions.DefaultViteServer,
			ProxyDevelopmentServerOptions.PnpmViteServer
		})
		{
			result.Add("/", "text/html", serverOptions);
			result.Add("/Index", "text/html", serverOptions);
			result.Add("/About", "text/html", serverOptions);
			result.Add("/Privacy", "text/html", serverOptions);
			result.Add("/Contact", "text/html", serverOptions);
			// The following file is for dev mode only; Vite should transpile the file on the fly
			result.Add("/src/main.tsx", "text/javascript", serverOptions);
		}
		return result;
	}
}
