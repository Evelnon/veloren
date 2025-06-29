using System;
using VelorenPort.CoreEngine;
using VelorenPort.NativeMath;
using VelorenPort.World.Site;
using VelorenPort.World.Civ;


namespace VelorenPort.World
{
    /// <summary>
    /// Simplified index keeping track of global world state.
    /// </summary>
    [Serializable]
    public class WorldIndex
    {
        public uint Seed { get; private set; }
        public float Time { get; set; }
        public Noise Noise { get; private set; }
        public WorldMap Map { get; } = new WorldMap();
        public Store<Site.Site> Sites { get; } = new();
        public Store<VelorenPort.CoreEngine.Npc> Npcs { get; } = new();
        public List<Site.TradingRoute> TradingRoutes { get; } = new();
        public List<Site.PopulationEvent> PopulationEvents { get; } = new();
        public Airships Airships { get; } = new();

        private ulong _nextUid;
        public Uid AllocateUid() => new Uid(_nextUid++);
        public Weather CurrentWeather { get; set; } = new Weather(0f, 0f, float2.zero);
        public Unity.Entities.EntityManager EntityManager { get; } = new Unity.Entities.EntityManager();

        public WorldIndex(uint seed)
        {
            Seed = seed;
            Noise = new Noise(seed);
        }
    }
}
