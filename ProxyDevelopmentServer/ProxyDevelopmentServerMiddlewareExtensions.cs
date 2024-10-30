using DarkPatterns.ProxyDevelopmentServer;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SpaServices;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// Extension methods for enabling Proxy development server middleware support.
/// </summary>
public static class ProxyDevelopmentServerMiddlewareExtensions
{
	/// <summary>
	/// Handles requests by passing them through to an instance of the Proxy server.
	/// This means you can always serve up-to-date CLI-built resources without having
	/// to run the Proxy server manually.
	///
	/// This feature should only be used in development. For production deployments, be
	/// sure not to enable the Proxy server.
	/// </summary>
	/// <param name="spaBuilder">The <see cref="ISpaBuilder"/>.</param>
	/// <param name="options">The cnofiguration for the proxy development server</param>
	public static void UseProxyDevelopmentServer(
		this ISpaBuilder spaBuilder,
		ProxyDevelopmentServerOptions options)
	{
		ArgumentNullException.ThrowIfNull(spaBuilder);

		var spaOptions = spaBuilder.Options;

		if (string.IsNullOrEmpty(spaOptions.SourcePath))
		{
			throw new InvalidOperationException($"To use {nameof(UseProxyDevelopmentServer)}, you must supply a non-empty value for the {nameof(SpaOptions.SourcePath)} property of {nameof(SpaOptions)} when calling {nameof(SpaApplicationBuilderExtensions.UseSpa)}.");
		}

		ProxyDevelopmentServerMiddleware.Attach(spaBuilder, options);
	}
}
