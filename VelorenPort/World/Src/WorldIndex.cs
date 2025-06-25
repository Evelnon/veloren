using System;

namespace VelorenPort.World {
    /// <summary>
    /// Indice reducido que mantiene el estado general del mundo.
    /// Sirve como punto de partida para la logica de generacion procedural.
    /// </summary>
    [Serializable]
    public class WorldIndex {
        public uint Seed { get; private set; }
        public float Time { get; set; }
        public Noise Noise { get; private set; }

        public WorldIndex(uint seed) {
            Seed = seed;
            Noise = new Noise(seed);
        }
    }
}
