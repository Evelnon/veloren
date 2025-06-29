using System.Net;
using System.Net.Sockets;

namespace Network.Tests;

internal static class TestUtils
{
    public static int GetFreePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    public static string GetRepoRoot()
    {
        var psi = new System.Diagnostics.ProcessStartInfo("git", "rev-parse --show-toplevel")
        {
            RedirectStandardOutput = true,
            UseShellExecute = false
        };
        using var proc = System.Diagnostics.Process.Start(psi)!;
        proc.WaitForExit();
        return proc.StandardOutput.ReadLine() ?? string.Empty;
    }
}
