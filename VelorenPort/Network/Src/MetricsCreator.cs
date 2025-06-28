using Prometheus;

namespace VelorenPort.Network;

internal static class MetricsCreator
{
    public static Counter CreateCounter(string name, string help) => Metrics.CreateCounter(name, help);
}
