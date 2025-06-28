using System;
using System.Threading;

namespace VelorenPort.Server.Sys {
    /// <summary>
    /// Periodically flushes modified data to disk. This is a very small
    /// equivalent of <c>persistence::Sys</c> from the Rust project. It triggers
    /// character and terrain persistence every 60 ticks.
    /// </summary>
    public class Persistence {
        private ulong _lastTick;

        public void Update(
            ulong tick,
            CharacterUpdater characterUpdater,
            TerrainPersistence terrainPersistence)
        {
            if (tick - _lastTick < 60)
                return;
            _lastTick = tick;

            try { characterUpdater.SaveAll(); } catch (Exception e) {
                Console.WriteLine($"[Persistence] Failed to save characters: {e.Message}");
            }

            try { terrainPersistence.FlushModified(); } catch (Exception e) {
                Console.WriteLine($"[Persistence] Failed to save terrain: {e.Message}");
            }
        }
    }
}

