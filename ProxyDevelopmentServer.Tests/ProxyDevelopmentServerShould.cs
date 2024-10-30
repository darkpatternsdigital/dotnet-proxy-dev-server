using DarkPatterns.SampleServer;

using Microsoft.AspNetCore.TestHost;

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
	[InlineData("/src/main.tsx", "application/javascript")]
	public async Task Get_endpoints_return_success_and_correct_content_type(string path, string contentType)
	{
		// Arrange
		var client = factory
			.WithWebHostBuilder(builder => builder.UseSolutionRelativeContentRoot("ProxyDevelopmentServer.Demo"))
			.CreateClient();

		// Act
		var response = await client.GetAsync(path);

		// Assert
		response.EnsureSuccessStatusCode(); // Status Code 200-299
		Assert.Equal(contentType, response.Content.Headers.ContentType?.MediaType);
	}

}
