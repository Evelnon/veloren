using System;
using Unity.Mathematics;

namespace VelorenPort.World
{
    /// <summary>
    /// Basic wildlife categories for procedural spawning.
    /// </summary>
    [Serializable]
    public enum FaunaKind
    {
        Generic,
        Wolf,
        Bear,
        Rabbit,
        Deer
    }

    /// <summary>
    /// Spawn data for a single animal.
    /// </summary>
    [Serializable]
    public struct FaunaSpawn
    {
        public int3 Position { get; set; }
        public FaunaKind Kind { get; set; }

        public FaunaSpawn(int3 position, FaunaKind kind)
        {
            Position = position;
            Kind = kind;
        }
    }
}
