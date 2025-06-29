using System;

namespace VelorenPort.World.Site
{
    /// <summary>
    /// Rules governing spawning of environment features around a site.
    /// Provides a simplified version of the Rust structure.
    /// </summary>
    [Serializable]
    public struct SpawnRules
    {
        public bool Trees;
        public float MaxWarp;
        public bool Paths;
        public bool Waypoints;

        public static SpawnRules Default => new SpawnRules
        {
            Trees = true,
            MaxWarp = 1f,
            Paths = true,
            Waypoints = true
        };

        public SpawnRules Combine(SpawnRules other)
        {
            return new SpawnRules
            {
                Trees = Trees && other.Trees,
                MaxWarp = Math.Min(MaxWarp, other.MaxWarp),
                Paths = Paths && other.Paths,
                Waypoints = Waypoints && other.Waypoints
            };
        }
    }
}
