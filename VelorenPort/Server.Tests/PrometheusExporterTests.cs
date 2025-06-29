using VelorenPort.Server.Sys;

namespace Server.Tests;

public class PrometheusExporterTests
{
    [Fact]
    public void FormatMetrics_ReturnsTickCount()
    {
        var metrics = new Metrics();
        for (int i = 0; i < 5; i++)
            metrics.RecordTick();
        var exporter = new PrometheusExporter(metrics, port: 0);
        string output = exporter.FormatMetrics();
        Assert.Contains("server_ticks_total 5", output);
    }
}
