namespace DarkPatterns.ProxyDevelopmentServer;

internal static class SolutionUtils
{
	public static string GetSolutionDirectory(
		string applicationBasePath,
		string solutionName = "*.sln")
	{
		var directoryInfo = new DirectoryInfo(applicationBasePath);
		do
		{
			var solutionPath = Directory.EnumerateFiles(directoryInfo.FullName, solutionName).FirstOrDefault();
			if (solutionPath != null)
			{
				return directoryInfo.FullName;
			}

			directoryInfo = directoryInfo.Parent;
		}
		while (directoryInfo is not null);

		throw new InvalidOperationException($"Solution root could not be located using application root {applicationBasePath}.");
	}
}
