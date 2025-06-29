using System;
using VelorenPort.CoreEngine;
using System.Collections.Generic;
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
        public List<Site.Caravan> Caravans { get; } = new();
        public List<Site.Economy.CaravanRoute> CaravanRoutes { get; } = new();
        public Site.Economy.EconomyContext EconomyContext { get; } = new();

        private ulong _nextUid;
        public Uid AllocateUid() => new Uid(_nextUid++);
        public Weather CurrentWeather { get; set; } = new Weather(0f, 0f, float2.zero);
        public Unity.Entities.EntityManager EntityManager { get; } = new Unity.Entities.EntityManager();

        public void AddTradingRoute(Site.TradingRoute route) => TradingRoutes.Add(route);
        public void RecordPopulationEvent(Site.PopulationEvent ev) => PopulationEvents.Add(ev);

        public WorldIndex(uint seed)
        {
            Seed = seed;
            Noise = new Noise(seed);
        }
    }
}
