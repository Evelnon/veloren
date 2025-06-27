using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using VelorenPort.CoreEngine;
using VelorenPort.Server;
using SqlLogMode = VelorenPort.Server.SqlLogMode;
using SqlLogModeExtensions = VelorenPort.Server.SqlLogModeExtensions;

namespace VelorenPort.CLI {
    /// <summary>
    /// Command line definitions mirroring <c>cli.rs</c> from the Rust project.
    /// Provides a minimal parser for command line arguments used to run the
    /// server from a console environment.
    /// </summary>
    public static class Cli {
        // -------------------- SqlLogMode --------------------
        // Use the server's SQL logging enumeration for consistent configuration.

        // -------------------- Admin --------------------
        public abstract record AdminCommand {
            public sealed record Add(string Username, AdminRole Role) : AdminCommand;
            public sealed record Remove(string Username) : AdminCommand;
        }

        // -------------------- Shutdown --------------------
        public abstract record ShutdownCommand {
            public sealed record Immediate : ShutdownCommand;
            public sealed record Graceful(uint Seconds, string Reason) : ShutdownCommand;
            public sealed record Cancel : ShutdownCommand;
        }

        // -------------------- SharedCommand --------------------
        public abstract record SharedCommand {
            public sealed record Admin(AdminCommand Command) : SharedCommand;
        }

        // -------------------- Message --------------------
        public abstract record Message {
            public sealed record Shared(SharedCommand Command) : Message;
            public sealed record Shutdown(ShutdownCommand Command) : Message;
            public sealed record LoadArea(uint ViewDistance) : Message;
            public sealed record SqlLogModeMsg(SqlLogMode Mode) : Message;
            public sealed record DisconnectAllClients : Message;
            public sealed record ListPlayers : Message;
            public sealed record ListLogs : Message;
            public sealed record SendGlobalMsg(string Msg) : Message;
        }

        public abstract record MessageReturn {
            public sealed record Players(List<string> Names) : MessageReturn;
            public sealed record Logs(List<string> Lines) : MessageReturn;
        }

        // -------------------- Argv structures --------------------
        public record BenchParams(uint ViewDistance, uint Duration);

        public abstract record ArgvCommand {
            public sealed record Shared(SharedCommand Command) : ArgvCommand;
            public sealed record Bench(BenchParams Params) : ArgvCommand;
        }

        public class ArgvApp {
            public bool Tui { get; set; }
            public bool NonInteractive { get; set; }
            public bool NoAuth { get; set; }
            public SqlLogMode SqlLog { get; set; } = SqlLogMode.Disabled;
            public ArgvCommand? Command { get; set; }
        }

        /// <summary>
        /// Parses command line arguments using a lightweight queue-based
        /// approach that mirrors the structure of the Rust CLI.
        /// </summary>
        public static ArgvApp Parse(string[] args) {
            var app = new ArgvApp();
            var queue = new Queue<string>(args);

            while (queue.Count > 0) {
                string arg = queue.Dequeue();
                switch (arg) {
                    case "--tui":
                    case "-t":
                        app.Tui = true;
                        break;
                    case "--non-interactive":
                    case "-n":
                        app.NonInteractive = true;
                        break;
                    case "--no-auth":
                        app.NoAuth = true;
                        break;
                    case "--sql-log-mode":
                    case "-s":
                        if (queue.Count == 0)
                            throw new ArgumentException("Missing value for --sql-log-mode");
                        var modeStr = queue.Dequeue();
                        if (!SqlLogModeExtensions.TryParse(modeStr, out var mode))
                            throw new ArgumentException($"Unknown sql log mode: {modeStr}");
                        app.SqlLog = mode;
                        break;
                    case "admin":
                        app.Command = new ArgvCommand.Shared(ParseAdmin(queue));
                        queue.Clear();
                        break;
                    case "bench":
                        app.Command = new ArgvCommand.Bench(ParseBench(queue));
                        queue.Clear();
                        break;
                    default:
                        throw new ArgumentException($"Unknown argument {arg}");
                }
            }

            return app;
        }

        private static SharedCommand.Admin ParseAdmin(Queue<string> queue) {
            if (queue.Count == 0)
                throw new ArgumentException("Missing admin subcommand");
            string sub = queue.Dequeue();
            return sub switch {
                "add" => new SharedCommand.Admin(ParseAdminAdd(queue)),
                "remove" => new SharedCommand.Admin(ParseAdminRemove(queue)),
                _ => throw new ArgumentException($"Unknown admin command {sub}")
            };
        }

        private static AdminCommand ParseAdminAdd(Queue<string> q) {
            if (q.Count < 2)
                throw new ArgumentException("admin add requires <username> <role>");
            string user = q.Dequeue();
            string roleStr = q.Dequeue();
            if (!AdminRoleHelper.TryParse(roleStr, out var role))
                throw new ArgumentException($"Invalid admin role: {roleStr}");
            return new AdminCommand.Add(user, role);
        }

        private static AdminCommand ParseAdminRemove(Queue<string> q) {
            if (q.Count < 1)
                throw new ArgumentException("admin remove requires <username>");
            string user = q.Dequeue();
            return new AdminCommand.Remove(user);
        }

        private static BenchParams ParseBench(Queue<string> q) {
            uint view = 0;
            uint dur = 0;
            while (q.Count > 0) {
                string arg = q.Dequeue();
                switch (arg) {
                    case "--view-distance":
                        if (q.Count == 0) throw new ArgumentException("Missing value for --view-distance");
                        view = uint.Parse(q.Dequeue());
                        break;
                    case "--duration":
                        if (q.Count == 0) throw new ArgumentException("Missing value for --duration");
                        dur = uint.Parse(q.Dequeue());
                        break;
                    default:
                        throw new ArgumentException($"Unknown bench argument {arg}");
                }
            }
            return new BenchParams(view, dur);

        /// <summary>
        /// Parses a command string as entered in the TUI and sends the resulting
        /// message to the provided queue.
        /// </summary>
        public static void ParseCommand(string input, System.Collections.Concurrent.BlockingCollection<Message> sender) {
            if (string.IsNullOrWhiteSpace(input)) return;
            try {
                var args = SplitArgs(input);
                var msg = ParseMessage(new Queue<string>(args));
                sender.Add(msg);
            } catch (Exception e) {
                Console.Error.WriteLine(e.Message);
            }
        }

        private static IEnumerable<string> SplitArgs(string input) {
            var list = new List<string>();
            var current = new System.Text.StringBuilder();
            bool inQuote = false;
            foreach (char c in input) {
                if (c == '"') { inQuote = !inQuote; continue; }
                if (char.IsWhiteSpace(c) && !inQuote) {
                    if (current.Length > 0) { list.Add(current.ToString()); current.Clear(); }
                    continue;
                }
                current.Append(c);
            }
            if (current.Length > 0) list.Add(current.ToString());
            return list;
        }

        private static Message ParseMessage(Queue<string> q) {
            if (q.Count == 0) throw new ArgumentException("Missing command");
            string cmd = q.Dequeue();
            return cmd switch {
                "shutdown" => new Message.Shutdown(ParseShutdown(q)),
                "admin" => new Message.Shared(ParseAdmin(q)),
                "load-area" => new Message.LoadArea(uint.Parse(q.Dequeue())),
                "sql-log-mode" => ParseSqlMode(q),
                "disconnect-all-clients" => new Message.DisconnectAllClients(),
                "list-players" => new Message.ListPlayers(),
                "list-logs" => new Message.ListLogs(),
                "send-global-msg" => new Message.SendGlobalMsg(string.Join(' ', q)),
                _ => throw new ArgumentException($"Unknown command {cmd}")
            };
        }

        private static Message ParseSqlMode(Queue<string> q) {
            if (q.Count == 0) throw new ArgumentException("Missing sql log mode");
            string val = q.Dequeue();
            if (!SqlLogModeExtensions.TryParse(val, out var mode))
                throw new ArgumentException($"Unknown sql log mode: {val}");
            return new Message.SqlLogModeMsg(mode);
        }

        private static ShutdownCommand ParseShutdown(Queue<string> q) {
            if (q.Count == 0) throw new ArgumentException("Missing shutdown subcommand");
            string sub = q.Dequeue();
            return sub switch {
                "immediate" => new ShutdownCommand.Immediate(),
                "graceful" => ParseShutdownGraceful(q),
                "cancel" => new ShutdownCommand.Cancel(),
                _ => throw new ArgumentException($"Unknown shutdown command {sub}")
            };
        }

        private static ShutdownCommand ParseShutdownGraceful(Queue<string> q) {
            if (q.Count == 0) throw new ArgumentException("shutdown graceful requires <seconds>");
            uint secs = uint.Parse(q.Dequeue());
            string reason = q.Count > 0 ? string.Join(' ', q) : "The server is shutting down";
            q.Clear();
            return new ShutdownCommand.Graceful(secs, reason);
        }
    }
}
