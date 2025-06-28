using Unity.Mathematics;

namespace VelorenPort.World
{
    /// <summary>
    /// Utility constants used across world generation modules.
    /// Mirrors a subset of world/src/util/mod.rs.
    /// </summary>
    public static class WorldUtil
    {
        public static readonly int2[] NEIGHBORS =
        {
            new int2(1, 0),
            new int2(1, 1),
            new int2(0, 1),
            new int2(-1, 1),
            new int2(-1, 0),
            new int2(-1, -1),
            new int2(0, -1),
            new int2(1, -1)
        };

        public static readonly int2[] LOCALITY =
        {
            new int2(0, 0),
            new int2(0, 1),
            new int2(1, 0),
            new int2(0, -1),
            new int2(-1, 0),
            new int2(1, 1),
            new int2(1, -1),
            new int2(-1, 1),
            new int2(-1, -1)
        };
    }
}
