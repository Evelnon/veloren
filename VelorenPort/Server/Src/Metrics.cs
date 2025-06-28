using System;
using System.Collections.Generic;
using System.Threading;

namespace VelorenPort.Server {
    // Minimal metric primitives used by server metrics
    public class IntCounter {
        private long _value;
        public void Inc() => Interlocked.Increment(ref _value);
        public void IncBy(long val) => Interlocked.Add(ref _value, val);
        public long Value => Interlocked.Read(ref _value);
    }

    public class IntGauge {
        private long _value;
        public void Set(long val) => Interlocked.Exchange(ref _value, val);
        public void IncBy(long val) => Interlocked.Add(ref _value, val);
        public long Value => Interlocked.Read(ref _value);
    }

    public class Gauge {
        private double _value;
        public void Set(double val) => Interlocked.Exchange(ref _value, val);
    }

    public class Histogram {
        public void Observe(double _value) { /* not implemented */ }
    }

    public class IntCounterVec {
        private readonly Dictionary<string, IntCounter> _counters = new();
        public IntCounter WithLabel(string label) {
            if (!_counters.TryGetValue(label, out var c)) {
                c = new IntCounter();
                _counters[label] = c;
            }
            return c;
        }
    }

    public class IntGaugeVec {
        private readonly Dictionary<string, IntGauge> _gauges = new();
        public IntGauge WithLabel(string label) {
            if (!_gauges.TryGetValue(label, out var g)) {
                g = new IntGauge();
                _gauges[label] = g;
            }
            return g;
        }
    }

    public class HistogramVec {
        private readonly Dictionary<string, Histogram> _hist = new();
        public Histogram WithLabel(string label) {
            if (!_hist.TryGetValue(label, out var h)) {
                h = new Histogram();
                _hist[label] = h;
            }
            return h;
        }
    }

    // Metric aggregates equivalent to server/src/metrics.rs
    public class PhysicsMetrics {
        public IntCounter EntityEntityCollisionChecksCount { get; } = new();
        public IntCounter EntityEntityCollisionsCount { get; } = new();
    }

    public class EcsSystemMetrics {
        public IntGaugeVec SystemStartTime { get; } = new();
        public IntGaugeVec SystemLengthTime { get; } = new();
        public GaugeVec SystemThreadAvg { get; } = new();
        public HistogramVec SystemLengthHist { get; } = new();
        public IntCounterVec SystemLengthCount { get; } = new();
    }

    public class GaugeVec {
        private readonly Dictionary<string, Gauge> _gauges = new();
        public Gauge WithLabel(string label) {
            if (!_gauges.TryGetValue(label, out var g)) {
                g = new Gauge();
                _gauges[label] = g;
            }
            return g;
        }
    }

    public class PlayerMetrics {
        public IntCounter ClientsConnected { get; } = new();
        public IntCounter PlayersConnected { get; } = new();
        public IntCounterVec ClientsDisconnected { get; } = new();
    }

    public class NetworkRequestMetrics {
        public IntCounter ChunksRequestDropped { get; } = new();
        public IntCounter ChunksServedFromMemory { get; } = new();
        public IntCounter ChunksGenerationTriggered { get; } = new();
        public IntCounter ChunksServedLossy { get; } = new();
        public IntCounter ChunksServedLossless { get; } = new();
        public IntCounter ChunksSerialisationRequests { get; } = new();
        public IntCounter ChunksDistinctSerialisationRequests { get; } = new();
    }

    public class ChunkGenMetrics {
        public IntCounter ChunksRequested { get; } = new();
        public IntCounter ChunksServed { get; } = new();
        public IntCounter ChunksCanceled { get; } = new();
    }

    public class JobMetrics {
        public HistogramVec JobQueriedHst { get; } = new();
        public HistogramVec JobExecutionHst { get; } = new();
    }

    public class TickMetrics {
        public IntGauge ChonksCount { get; } = new();
        public IntGauge ChunksCount { get; } = new();
        public IntGauge ChunkGroupsCount { get; } = new();
        public IntGauge EntityCount { get; } = new();
        public IntGaugeVec TickTime { get; } = new();
        public IntGaugeVec StateTickTime { get; } = new();
        public Histogram TickTimeHist { get; } = new();
        public IntGauge BuildInfo { get; } = new();
        public IntGauge StartTime { get; } = new();
        public Gauge TimeOfDay { get; } = new();
        public IntGauge LightCount { get; } = new();
    }

    public class ServerEventMetrics {
        public IntCounterVec EventCount { get; } = new();
    }

    public class QueryServerMetrics {
        public IntCounter ReceivedPackets { get; } = new();
        public IntCounter DroppedPackets { get; } = new();
        public IntCounter InvalidPackets { get; } = new();
        public IntCounter ProccessingErrors { get; } = new();
        public IntCounter InfoRequests { get; } = new();
        public IntCounter InitRequests { get; } = new();
        public IntCounter SentResponses { get; } = new();
        public IntCounter FailedResponses { get; } = new();
        public IntCounter TimedOutResponses { get; } = new();
        public IntCounter Ratelimited { get; } = new();

        public void Apply(QueryServerMetrics other) {
            ReceivedPackets.IncBy(other.ReceivedPacketsValue);
            DroppedPackets.IncBy(other.DroppedPacketsValue);
            InvalidPackets.IncBy(other.InvalidPacketsValue);
            ProccessingErrors.IncBy(other.ProccessingErrorsValue);
            InfoRequests.IncBy(other.InfoRequestsValue);
            InitRequests.IncBy(other.InitRequestsValue);
            SentResponses.IncBy(other.SentResponsesValue);
            FailedResponses.IncBy(other.FailedResponsesValue);
            TimedOutResponses.IncBy(other.TimedOutResponsesValue);
            Ratelimited.IncBy(other.RatelimitedValue);
        }

        private long ReceivedPacketsValue => ReceivedPackets.Value;
        private long DroppedPacketsValue => DroppedPackets.Value;
        private long InvalidPacketsValue => InvalidPackets.Value;
        private long ProccessingErrorsValue => ProccessingErrors.Value;
        private long InfoRequestsValue => InfoRequests.Value;
        private long InitRequestsValue => InitRequests.Value;
        private long SentResponsesValue => SentResponses.Value;
        private long FailedResponsesValue => FailedResponses.Value;
        private long TimedOutResponsesValue => TimedOutResponses.Value;
        private long RatelimitedValue => Ratelimited.Value;
    }
}
