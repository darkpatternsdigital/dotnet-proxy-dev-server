namespace DarkPatterns.ProxyDevelopmentServer;

public record ProxyDevelopmentServerOptions
{
	public static readonly ProxyDevelopmentServerOptions DefaultViteServer = new()
	{
		IsScriptFile = true,
		BaseCommand = "node_modules/.bin/vite",
		Parameters = "--port {port}",
		ReadyText = "ready in",
		Timeout = TimeSpan.FromSeconds(5)
	};
	public static readonly ProxyDevelopmentServerOptions PnpmViteServer = new()
	{
		IsScriptFile = false,
		BaseCommand = "pnpm",
		Parameters = "exec vite --port {port}",
		ReadyText = "ready in",
		Timeout = TimeSpan.FromSeconds(5)
	};


	/// <summary>
	/// Set to true when a BaseCommand refers to a file
	/// </summary>
	public required bool IsScriptFile { get; init; }
	/// <summary>
	/// The name of the executable to launch.
	/// </summary>
	public required string BaseCommand { get; init; }
	/// <summary>
	/// The parameters to pass to the process. Will replace `{port}` with the
	/// port number.
	/// </summary>
	public required string Parameters { get; init; }

	/// <summary>
	/// When matched, the proxy port will be considered ready.
	/// </summary>
	public required string ReadyText { get; init; }

	/// <summary>
	/// If the <see cref="ReadyText"/> is not found by the timeout, the application will be aborted.
	/// </summary>
	public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(5); // This is a development-time only feature, so a very long timeout is fine

	public delegate (string command, string completeArguments) ToWindowsCommandCallback(ProxyDevelopmentServerOptions options, string fullWorkingDirectory);
	public ToWindowsCommandCallback ToWindowsCommand { get; init; } = DefaultToWindowsCommand;

	public static (string command, string completeArguments) DefaultToWindowsCommand(ProxyDevelopmentServerOptions options, string fullWorkingDirectory)
	{
		if (!options.IsScriptFile)
			return (options.BaseCommand, options.Parameters);

		var fullPathToCommand = Path.GetFullPath(options.BaseCommand, fullWorkingDirectory);

		// On Windows, script files can't be executed directly (except with
		// UseShellExecute=true, but that's no good, because it prevents
		// capturing stdio). So we need to invoke it via "cmd /c". Cmd also does
		// not auto-adjust forward-slashes to Window's backslashes.
		return ("cmd", $"/c \"{fullPathToCommand.Replace('/', '\\')}\" {options.Parameters}");
	}
}
