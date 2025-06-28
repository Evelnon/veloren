namespace VelorenPort.Server {
    /// <summary>Temporary stubs while the server code is migrated.</summary>
    public enum SqlLogMode { Disabled, Profile, Trace }
    public static class SqlLogModeExtensions {
        public static bool TryParse(string? value, out SqlLogMode mode) {
            mode = SqlLogMode.Disabled;
            if (string.IsNullOrWhiteSpace(value)) return false;
            value = value.ToLowerInvariant();
            if (value == "profile") { mode = SqlLogMode.Profile; return true; }
            if (value == "trace") { mode = SqlLogMode.Trace; return true; }
            if (value == "disabled") { mode = SqlLogMode.Disabled; return true; }
            return false;
        }
    }

    public class GameServer {
        public GameServer(VelorenPort.Network.Pid pid, System.TimeSpan tickRate, int port) {}
        public System.Threading.Tasks.Task RunAsync(VelorenPort.Network.ListenAddr addr, System.Threading.CancellationToken t) => System.Threading.Tasks.Task.CompletedTask;
        public void NotifyPlayers(string msg) {}
    }

    /// <summary>Very small test world used by unit tests.</summary>
    public class TestWorld {
        public readonly struct IndexRef { }
        public readonly struct IndexOwned {
            public IndexRef AsIndexRef() => new IndexRef();
        }
        public static (TestWorld world, IndexOwned index) Generate(uint seed) => (new TestWorld(), new IndexOwned());
        public void Tick(System.TimeSpan dt) { }
    }

    public class Placeholder { }
}
