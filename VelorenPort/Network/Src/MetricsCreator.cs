using Prometheus;

namespace VelorenPort.Network;

internal static class MetricsCreator
{
    public static Counter CreateCounter(string name, string help, params string[] labelNames)
        => Metrics.CreateCounter(name, help, new CounterConfiguration { LabelNames = labelNames });

    public static Gauge CreateGauge(string name, string help, params string[] labelNames)
        => Metrics.CreateGauge(name, help, new GaugeConfiguration { LabelNames = labelNames });
}
