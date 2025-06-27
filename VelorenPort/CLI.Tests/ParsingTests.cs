using VelorenPort.CLI;
using VelorenPort.Server;

namespace CLI.Tests;

public class CliParsingTests
{
    [Fact]
    public void Parse_NoAuthFlag_SetsNoAuthTrue()
    {
        var app = Cli.Parse(new[] { "--no-auth" });
        Assert.True(app.NoAuth);
    }

    [Fact]
    public void Parse_NoArguments_DefaultsAreFalse()
    {
        var app = Cli.Parse(Array.Empty<string>());
        Assert.False(app.NoAuth);
        Assert.False(app.Tui);
        Assert.False(app.NonInteractive);
        Assert.Equal(SqlLogMode.Disabled, app.SqlLog);
    }

    [Theory]
    [InlineData("disabled", SqlLogMode.Disabled)]
    [InlineData("trace", SqlLogMode.Trace)]
    public void Parse_SqlLogMode_ParsesCorrectly(string mode, SqlLogMode expected)
    {
        var app = Cli.Parse(new[] {"--sql-log-mode", mode});
        Assert.Equal(expected, app.SqlLog);
    }
}
