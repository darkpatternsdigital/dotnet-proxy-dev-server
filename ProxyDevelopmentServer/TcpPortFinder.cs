using System.Net;
using System.Net.Sockets;

namespace DarkPatterns.ProxyDevelopmentServer;

internal static class TcpPortFinder
{
	public static int FindAvailablePort()
	{
		using var listener = new TcpListener(IPAddress.Loopback, 0);
		listener.Start();
		try
		{
			return ((IPEndPoint)listener.LocalEndpoint).Port;
		}
		finally
		{
			listener.Stop();
		}
	}
}
