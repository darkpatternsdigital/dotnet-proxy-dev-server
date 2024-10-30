using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SpaServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DarkPatterns.ProxyDevelopmentServer;

internal static class ProxyDevelopmentServerMiddleware
{
	private static readonly TimeSpan RegexMatchTimeout = TimeSpan.FromSeconds(5); // This is a development-time only feature, so a very long timeout is fine

	public static void Attach(
		ISpaBuilder spaBuilder,
		string baseCommand,
		string parameters)
	{
		var pkgManagerCommand = baseCommand;
		var sourcePath = spaBuilder.Options.SourcePath;
		var devServerPort = spaBuilder.Options.DevServerPort;
		if (string.IsNullOrEmpty(sourcePath))
		{
			throw new ArgumentException("Property 'SourcePath' cannot be null or empty", nameof(spaBuilder));
		}

		if (string.IsNullOrEmpty(parameters))
		{
			throw new ArgumentException("Cannot be null or empty", nameof(parameters));
		}

		// Start Process and attach to middleware pipeline
		var appBuilder = spaBuilder.ApplicationBuilder;
		var applicationStoppingToken = appBuilder.ApplicationServices.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping;
		var logger = LoggerFinder.GetOrCreateLogger(appBuilder, typeof(ProxyDevelopmentServerMiddleware).FullName!);
		var diagnosticSource = appBuilder.ApplicationServices.GetRequiredService<DiagnosticSource>();
		var portTask = StartCreateProxyAppServerAsync(sourcePath, parameters, pkgManagerCommand, devServerPort, logger, diagnosticSource, applicationStoppingToken);

		SpaProxyingExtensions.UseProxyToSpaDevelopmentServer(spaBuilder, async () =>
		{
			// On each request, we create a separate startup task with its own timeout. That way, even if
			// the first request times out, subsequent requests could still work.
			var timeout = spaBuilder.Options.StartupTimeout;
			var port = await portTask.WithTimeout(timeout, "The Proxy server did not start listening for requests " +
				$"within the timeout period of {timeout.TotalSeconds} seconds. " +
				"Check the log output for error information.").ConfigureAwait(false);

			// Everything we proxy is hardcoded to target http://localhost because:
			// - the requests are always from the local machine (we're not accepting remote
			//   requests that go directly to the Proxy server)
			// - given that, there's no reason to use https, and we couldn't even if we
			//   wanted to, because in general the Proxy server has no certificate
			return new UriBuilder("http", "localhost", port).Uri;
		});
	}

	private static async Task<int> StartCreateProxyAppServerAsync(
		string sourcePath, string originalParameters, string command, int portNumber, ILogger logger, DiagnosticSource diagnosticSource, CancellationToken applicationStoppingToken)
	{
		if (portNumber == default(int))
		{
			portNumber = TcpPortFinder.FindAvailablePort();
		}
		if (logger.IsEnabled(LogLevel.Information))
		{
			logger.LogInformation("Starting Proxy server on port {Port}...", portNumber);
		}

		var parameters = originalParameters.Replace("{port}", portNumber.ToString(CultureInfo.InvariantCulture), StringComparison.InvariantCulture);

		var envVars = new Dictionary<string, string>();
#pragma warning disable CA2000 // Dispose objects before losing scope - this script needs to continue running after this function finishes
		var scriptRunner = new ProxyScriptRunner(
			sourcePath, command, parameters, envVars, diagnosticSource, applicationStoppingToken);
#pragma warning restore CA2000 // Dispose objects before losing scope
		scriptRunner.AttachToLogger(logger);

		using (var stdErrReader = new EventedStreamStringReader(scriptRunner.StdErr))
		{
			try
			{
				// Although the Proxy dev server may eventually tell us the URL it's listening on,
				// it doesn't do so until it's finished compiling, and even then only if there were
				// no compiler warnings. So instead of waiting for that, consider it ready as soon
				// as it starts listening for requests.
				await scriptRunner.StdOut.WaitForMatch(
					new Regex("ready in", RegexOptions.None, RegexMatchTimeout)).ConfigureAwait(true);
			}
			catch (EndOfStreamException ex)
			{
				throw new InvalidOperationException(
					$"The command '{command} {parameters}' exited without indicating that the " +
					"Proxy server was listening for requests. The error output was: " +
					$"{stdErrReader.ReadAsString()}", ex);
			}
		}

		return portNumber;
	}
}
