using VelorenPort.CoreEngine;

namespace CoreEngine.Tests;

public class VersionInfoTests
{
    [Fact]
    public void DisplayVersion_UsesEnvVars()
    {
        Environment.SetEnvironmentVariable("VELOREN_GIT_VERSION", "abcdef12/2025-06-28-12:34");
        Environment.SetEnvironmentVariable("VELOREN_GIT_TAG", "v0.1.0");
        Assert.Equal("Pre-Alpha-v0.1.0", VersionInfo.DisplayVersion);
        Assert.Equal("abcdef12", VersionInfo.GitHash);
        Assert.Equal("Pre-Alpha-v0.1.0 (abcdef12/2025-06-28-12:34)", VersionInfo.DisplayVersionLong);
    }
}
