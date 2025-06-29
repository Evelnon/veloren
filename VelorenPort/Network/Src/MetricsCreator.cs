using Prometheus;

namespace VelorenPort.Network;

internal static partial class MetricsCreator
{
    public static Counter CreateCounter(string name, string help, params string[] labelNames)
    {
        var c = Metrics.CreateCounter(name, help, new CounterConfiguration { LabelNames = labelNames });
        OnCounterCreated(c, name);
        return c;
    }

    public static Gauge CreateGauge(string name, string help, params string[] labelNames)
    {
        var g = Metrics.CreateGauge(name, help, new GaugeConfiguration { LabelNames = labelNames });
        OnGaugeCreated(g, name);
        return g;
    }

    public static Histogram CreateHistogram(string name, string help, params string[] labelNames)
    {
        var h = Metrics.CreateHistogram(name, help, new HistogramConfiguration { LabelNames = labelNames });
        OnHistogramCreated(h, name);
        return h;
    }

    static partial void OnCounterCreated(Counter counter, string name);
    static partial void OnGaugeCreated(Gauge gauge, string name);
    static partial void OnHistogramCreated(Histogram histogram, string name);
}
