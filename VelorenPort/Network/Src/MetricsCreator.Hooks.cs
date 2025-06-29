namespace VelorenPort.Network;

using System;
using Prometheus;

internal static partial class MetricsCreator
{
    public static event Action<Counter, string>? CounterCreated;
    public static event Action<Gauge, string>? GaugeCreated;
    public static event Action<Histogram, string>? HistogramCreated;

    static partial void OnCounterCreated(Counter counter, string name)
        => CounterCreated?.Invoke(counter, name);

    static partial void OnGaugeCreated(Gauge gauge, string name)
        => GaugeCreated?.Invoke(gauge, name);

    static partial void OnHistogramCreated(Histogram histogram, string name)
        => HistogramCreated?.Invoke(histogram, name);
}
