namespace DarkPatterns.ProxyDevelopmentServer;

#pragma warning disable CA1849 // Call async methods when in an async method
internal static class TaskTimeoutExtensions
{
	public static async Task WithTimeout(this Task task, TimeSpan timeoutDelay, string message)
	{
		if (task == await Task.WhenAny(task, Task.Delay(timeoutDelay)).ConfigureAwait(false))
		{
			task.Wait(); // Allow any errors to propagate
		}
		else
		{
			throw new TimeoutException(message);
		}
	}

	public static async Task<T> WithTimeout<T>(this Task<T> task, TimeSpan timeoutDelay, string message)
	{
		if (task == await Task.WhenAny(task, Task.Delay(timeoutDelay)).ConfigureAwait(false))
		{
			return task.Result;
		}
		else
		{
			throw new TimeoutException(message);
		}
	}
}
#pragma warning restore CA1849 // Call async methods when in an async method
