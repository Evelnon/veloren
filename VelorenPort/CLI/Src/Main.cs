using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using VelorenPort.CoreEngine;
using VelorenPort.Network;
using VelorenPort.Server;

namespace VelorenPort.CLI {
    /// <summary>
    /// Entrypoint that mirrors <c>server-cli/src/main.rs</c> with a simplified
    /// control loop. It parses command line arguments, loads settings and runs
    /// the <see cref="GameServer"/> until a shutdown command is received.
    /// </summary>
    public static class MainClass {
        private static readonly TimeSpan TickRate = TimeSpan.FromSeconds(1.0 / 30.0);

        public static async Task<int> Main(string[] args) {
            // Parse command line
            var app = Cli.Parse(args);
            bool basic = !app.Tui || app.Command != null;
            bool nonInteractive = app.NonInteractive;
            basic |= nonInteractive;

            // Initialize components
            var shutdownFlag = new AtomicBool();
            using var tui = !nonInteractive ? Tui.Run(basic) : null;
            var settings = Settings.Load();

            var server = new GameServer(Pid.NewPid(), TickRate, 0);
            var cts = new CancellationTokenSource();
            var serverTask = server.RunAsync(new ListenAddr.Tcp(new IPEndPoint(IPAddress.Loopback, 14004)), cts.Token);
            var shutdown = new ShutdownCoordinator(shutdownFlag);

            try {
                while (!cts.IsCancellationRequested) {
                    // Handle TUI commands
                    if (tui != null && tui.MsgR.TryTake(out var msg, 50)) {
                        if (HandleMessage(server, shutdown, msg, cts))
                            break;
                    }

                    if (shutdown.Check(server, settings))
                        break;
                }
            } finally {
                cts.Cancel();
                await serverTask;
                tui?.Dispose();
            }

            return 0;
        }

        private static bool HandleMessage(GameServer server, ShutdownCoordinator shutdown, Cli.Message msg, CancellationTokenSource cts) {
            switch (msg) {
                case Cli.Message.Shutdown(var cmd):
                    return HandleShutdown(server, shutdown, cmd, cts);
                case Cli.Message.SendGlobalMsg(var text):
                    server.NotifyPlayers(text);
                    break;
                default:
                    Console.Error.WriteLine($"Unsupported command: {msg.GetType().Name}");
                    break;
            }
            return false;
        }

        private static bool HandleShutdown(GameServer server, ShutdownCoordinator shutdown, Cli.ShutdownCommand cmd, CancellationTokenSource cts) {
            switch (cmd) {
                case Cli.ShutdownCommand.Immediate:
                    cts.Cancel();
                    return true;
                case Cli.ShutdownCommand.Graceful(var secs, var reason):
                    shutdown.InitiateShutdown(server, TimeSpan.FromSeconds(secs), reason);
                    break;
                case Cli.ShutdownCommand.Cancel:
                    shutdown.AbortShutdown(server);
                    break;
            }
            return false;
        }
    }
}
