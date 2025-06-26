using System;
using VelorenPort.CoreEngine;
using VelorenPort.Server;

namespace VelorenPort.CLI {
    /// <summary>
    /// Coordinates graceful shutdown of the server, matching the original Rust logic.
    /// </summary>
    public class ShutdownCoordinator {
        private DateTime _lastShutdownMsg;
        private TimeSpan _msgInterval;
        private DateTime? _shutdownInitiatedAt;
        private TimeSpan _shutdownGracePeriod;
        private string _shutdownMessage = string.Empty;
        private readonly AtomicBool _shutdownSignal;

        public ShutdownCoordinator(AtomicBool shutdownSignal) {
            _shutdownSignal = shutdownSignal;
            _lastShutdownMsg = DateTime.UtcNow;
            _msgInterval = TimeSpan.FromSeconds(30);
        }

        public void InitiateShutdown(GameServer server, TimeSpan gracePeriod, string message) {
            if (_shutdownInitiatedAt == null) {
                _shutdownGracePeriod = gracePeriod;
                _shutdownInitiatedAt = DateTime.UtcNow;
                _shutdownMessage = message;
                SendShutdownMsg(server);
            } else {
                Console.Error.WriteLine("Shutdown already in progress");
            }
        }

        public void AbortShutdown(GameServer server) {
            if (_shutdownInitiatedAt != null) {
                _shutdownInitiatedAt = null;
                SendMsg(server, "The shutdown has been aborted.");
            } else {
                Console.Error.WriteLine("There is no shutdown in progress");
            }
        }

        /// <summary>
        /// Processes shutdown timers and signals. Returns true when the server should exit.
        /// </summary>
        public bool Check(GameServer server, Settings settings) {
            CheckShutdownSignal(server, settings);

            if (_shutdownInitiatedAt != null) {
                if (DateTime.UtcNow > _shutdownInitiatedAt.Value + _shutdownGracePeriod) {
                    Console.WriteLine("Shutting down");
                    return true;
                }

                var remaining = TimeUntilShutdown();
                if (remaining != null && remaining.Value <= TimeSpan.FromSeconds(10))
                    _msgInterval = TimeSpan.FromSeconds(1);

                if (_lastShutdownMsg + _msgInterval <= DateTime.UtcNow)
                    SendShutdownMsg(server);
            }

            return false;
        }

        private void CheckShutdownSignal(GameServer server, Settings settings) {
            if (_shutdownSignal.Load() && _shutdownInitiatedAt == null) {
                Console.WriteLine("Received shutdown signal, initiating graceful shutdown");
                InitiateShutdown(server,
                    TimeSpan.FromSeconds(settings.UpdateShutdownGracePeriodSecs),
                    settings.UpdateShutdownMessage);
                _shutdownSignal.Store(false);
            }
        }

        private void SendShutdownMsg(GameServer server) {
            var remaining = TimeUntilShutdown();
            if (remaining != null) {
                string msg = $"{_shutdownMessage} in {DurationToText(remaining.Value)}";
                SendMsg(server, msg);
                _lastShutdownMsg = DateTime.UtcNow;
            }
        }

        private TimeSpan? TimeUntilShutdown() {
            if (_shutdownInitiatedAt is DateTime initiated) {
                var shutdownTime = initiated + _shutdownGracePeriod;
                var diff = shutdownTime - DateTime.UtcNow;
                return diff > TimeSpan.Zero ? diff : TimeSpan.Zero;
            }
            return null;
        }

        private static void SendMsg(GameServer server, string msg) {
            server.NotifyPlayers(msg);
        }

        private static string DurationToText(TimeSpan duration) {
            int secs = (int)Math.Round(duration.TotalSeconds) % 60;
            int mins = (int)Math.Round(duration.TotalMinutes);
            string text = string.Empty;
            if (mins > 0) text += $"{mins}m";
            if (secs > 0) text += $"{secs}s";
            return text;
        }
    }
}
