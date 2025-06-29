using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VelorenPort.Server.Sys {
    /// <summary>
    /// Very small HTTP exporter that exposes server metrics in the
    /// Prometheus text exposition format. Only a handful of metrics are
    /// provided for now.
    /// </summary>
    public class PrometheusExporter : IDisposable {
        private readonly Metrics _metrics;
        private readonly HttpListener _listener = new();
        private CancellationTokenSource? _cts;

        public PrometheusExporter(Metrics metrics, int port = 9090) {
            _metrics = metrics;
            _listener.Prefixes.Add($"http://localhost:{port}/");
        }

        /// <summary>Start listening for scrape requests.</summary>
        public void Start() {
            _cts = new CancellationTokenSource();
            _listener.Start();
            Task.Run(() => RunAsync(_cts.Token));
        }

        private async Task RunAsync(CancellationToken token) {
            while (!token.IsCancellationRequested) {
                var ctx = await _listener.GetContextAsync().ConfigureAwait(false);
                var data = Encoding.UTF8.GetBytes(FormatMetrics());
                ctx.Response.ContentType = "text/plain; version=0.0.4";
                ctx.Response.ContentLength64 = data.Length;
                await ctx.Response.OutputStream.WriteAsync(data, 0, data.Length, token).ConfigureAwait(false);
                ctx.Response.Close();
            }
        }

        /// <summary>Render the current metrics in Prometheus format.</summary>
        public string FormatMetrics() {
            var sb = new StringBuilder();
            sb.AppendLine("# TYPE server_ticks_total counter");
            sb.Append("server_ticks_total ").Append(_metrics.Ticks).Append('\n');
            return sb.ToString();
        }

        public void Dispose() {
            if (_listener.IsListening) _listener.Stop();
            _cts?.Cancel();
        }
    }
}
