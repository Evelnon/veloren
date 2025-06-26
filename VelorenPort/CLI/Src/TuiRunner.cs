using System;
using System.Collections.Concurrent;
using System.Threading;

namespace VelorenPort.CLI {
    /// <summary>
    /// Provides a very small console based interface to send commands to the
    /// running server. It mirrors <c>tui_runner.rs</c> in behaviour by spawning
    /// a background thread which reads from standard input.
    /// </summary>
    public sealed class Tui : IDisposable {
        public BlockingCollection<Cli.Message> MsgR { get; }
        private readonly Thread _background;
        private readonly bool _basic;
        private readonly CancellationTokenSource _cts = new();

        private Tui(bool basic, BlockingCollection<Cli.Message> msgQ, Thread bg) {
            _basic = basic;
            MsgR = msgQ;
            _background = bg;
        }

        public static Tui Run(bool basic) {
            var queue = new BlockingCollection<Cli.Message>();
            var tui = new Tui(basic, queue, new Thread(() => Worker(basic, queue))
            {
                IsBackground = true,
                Name = "tui_runner"
            });
            tui._background.Start();
            return tui;
        }

        private static void Worker(bool basic, BlockingCollection<Cli.Message> q) {
            if (basic) {
                while (true) {
                    var line = Console.ReadLine();
                    if (line == null) break;
                    Cli.ParseCommand(line.Trim(), q);
                }
            } else {
                var input = string.Empty;
                Console.TreatControlCAsInput = true;
                while (true) {
                    if (!Console.KeyAvailable) {
                        Thread.Sleep(50);
                        continue;
                    }
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Enter) {
                        Console.WriteLine();
                        Cli.ParseCommand(input, q);
                        input = string.Empty;
                    } else if (key.Key == ConsoleKey.Backspace) {
                        if (input.Length > 0) {
                            input = input[..^1];
                            Console.Write("\b \b");
                        }
                    } else if (key.Modifiers.HasFlag(ConsoleModifiers.Control) && key.Key == ConsoleKey.C) {
                        q.Add(new Cli.Message.Shutdown(new Cli.ShutdownCommand.Immediate()));
                    } else if (key.KeyChar != '\0') {
                        input += key.KeyChar;
                        Console.Write(key.KeyChar);
                    }
                }
            }
        }

        public static void Shutdown(bool basic) {
            if (!basic) {
                Console.WriteLine();
                Console.ResetColor();
            }
        }

        public void Dispose() {
            _cts.Cancel();
            if (_background.IsAlive)
                _background.Interrupt();
            _background.Join();
            Shutdown(_basic);
        }
    }
}
