using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

using Microsoft.Extensions.Logging;

namespace DarkPatterns.ProxyDevelopmentServer;

/// <summary>
/// Executes an executable in the background, capturing any output written to stdio.
/// </summary>
internal sealed class ProxyScriptRunner : IDisposable
{
	const string diagnosticSourceName = "DarkPatterns.ProxyDevelopmentServer.ProxyScriptRunner.Started";

	private Process? _process;
	public EventedStreamReader StdOut { get; }
	public EventedStreamReader StdErr { get; }

	private static readonly Regex AnsiColorRegex = new Regex("\x001b\\[[0-9;]*m", RegexOptions.None, TimeSpan.FromSeconds(1));

	public ProxyScriptRunner(string workingDirectory, ProxyDevelopmentServerOptions options, IDictionary<string, string>? envVars, DiagnosticSource diagnosticSource, CancellationToken applicationStoppingToken)
	{
		if (string.IsNullOrEmpty(workingDirectory))
		{
			throw new ArgumentException("Cannot be null or empty.", nameof(workingDirectory));
		}

		if (string.IsNullOrEmpty(options.Parameters))
		{
			throw new ArgumentException("Parameters in options cannot be null or empty.", nameof(options));
		}

		if (string.IsNullOrEmpty(options.BaseCommand))
		{
			throw new ArgumentException("BaseCommand in options cannot be null or empty.", nameof(options));
		}

		var fullWorkingDirectory = Path.GetFullPath(workingDirectory, Directory.GetCurrentDirectory());
		var (command, arguments) = GetCommandAndArgs(options, fullWorkingDirectory);

		var processStartInfo = new ProcessStartInfo(command)
		{
			Arguments = arguments,
			UseShellExecute = false,
			RedirectStandardInput = true,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			WorkingDirectory = fullWorkingDirectory,
		};

		if (envVars != null)
		{
			foreach (var keyValuePair in envVars)
			{
				processStartInfo.Environment[keyValuePair.Key] = keyValuePair.Value;
			}
		}

		_process = LaunchProxyProcess(processStartInfo, options.BaseCommand);
		StdOut = new EventedStreamReader(_process.StandardOutput);
		StdErr = new EventedStreamReader(_process.StandardError);

		applicationStoppingToken.Register(((IDisposable)this).Dispose);

		if (diagnosticSource.IsEnabled(diagnosticSourceName))
		{
			WriteDiagnosticEvent(
				diagnosticSource,
				diagnosticSourceName,
				new
				{
					processStartInfo = processStartInfo,
					process = _process
				});
		}

		[UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026",
			Justification = "The values being passed into Write have the commonly used properties being preserved with DynamicDependency.")]
		static void WriteDiagnosticEvent<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TValue>(DiagnosticSource diagnosticSource, string name, TValue value)
			=> diagnosticSource.Write(name, value);
	}

	private static (string command, string completeArguments) GetCommandAndArgs(ProxyDevelopmentServerOptions options, string fullWorkingDirectory)
	{
		if (options.IsScriptFile)
		{
			var fullPathToCommand = Path.GetFullPath(options.BaseCommand, fullWorkingDirectory);
			if (OperatingSystem.IsWindows())
			{
				return options.ToWindowsCommand(options, fullWorkingDirectory);
			}
			else
			{
				return (fullPathToCommand, options.Parameters);
			}
		}
		else
		{
			return (options.BaseCommand, options.Parameters);
		}
	}

	public void AttachToLogger(ILogger logger)
	{
		// When the node task emits complete lines, pass them through to the real logger
		StdOut.OnReceivedLine += line =>
		{
			if (!string.IsNullOrWhiteSpace(line) && logger.IsEnabled(LogLevel.Information))
			{
				// Node tasks commonly emit ANSI colors, but it wouldn't make sense to forward
				// those to loggers (because a logger isn't necessarily any kind of terminal)
				logger.LogInformation("proxy: {StdOut}", StripAnsiColors(line));
			}
		};

		StdErr.OnReceivedLine += line =>
		{
			if (!string.IsNullOrWhiteSpace(line))
			{
				logger.LogError("proxy: {StdError}", StripAnsiColors(line));
			}
		};

		// But when it emits incomplete lines, assume this is progress information and
		// hence just pass it through to StdOut regardless of logger config.
		StdErr.OnReceivedChunk += chunk =>
		{
			Debug.Assert(chunk.Array != null);

			var containsNewline = Array.IndexOf(
				chunk.Array, '\n', chunk.Offset, chunk.Count) >= 0;
			if (!containsNewline)
			{
				Console.Write(chunk.Array, chunk.Offset, chunk.Count);
			}
		};
	}

	private static string StripAnsiColors(string line)
		=> AnsiColorRegex.Replace(line, string.Empty);

	private static Process LaunchProxyProcess(ProcessStartInfo startInfo, string commandName)
	{
		try
		{
			var process = Process.Start(startInfo)!;

			// See equivalent comment in OutOfProcessNodeInstance.cs for why
			process.EnableRaisingEvents = true;

			return process;
		}
		catch (Exception ex)
		{
			var message = $"Failed to start '{commandName}'. To resolve this:.\n\n"
						+ $"[1] Ensure that '{commandName}' is installed and can be found in one of the PATH directories.\n"
						+ $"    Current PATH enviroment variable is: {Environment.GetEnvironmentVariable("PATH")}\n"
						+ "    Make sure the executable is in one of those directories, or update your PATH.\n\n"
						+ "[2] See the InnerException for further details of the cause.";
			throw new InvalidOperationException(message, ex);
		}
	}

	void IDisposable.Dispose()
	{
		if (_process != null && !_process.HasExited)
		{
			_process.Kill(entireProcessTree: true);
			_process.Dispose();
			_process = null;
		}
	}
}
