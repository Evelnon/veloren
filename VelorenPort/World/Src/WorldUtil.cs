using VelorenPort.NativeMath;

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

        public static readonly int2[] CARDINALS =
        {
            new int2(1, 0),
            new int2(0, 1),
            new int2(-1, 0),
            new int2(0, -1)
        };

        public static readonly int2[] DIRS =
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

        public static readonly int2[] DIAGONALS =
        {
            new int2(1, 1),
            new int2(-1, 1),
            new int2(-1, -1),
            new int2(1, -1)
        };

        public static readonly int3[] NEIGHBORS3 =
        {
            new int3(0,0,-1),
            new int3(0,0,1),
            new int3(0,-1,0),
            new int3(0,-1,-1),
            new int3(0,-1,1),
            new int3(0,1,0),
            new int3(0,1,-1),
            new int3(0,1,1),
            new int3(-1,0,0),
            new int3(-1,0,-1),
            new int3(-1,0,1),
            new int3(-1,-1,0),
            new int3(-1,-1,-1),
            new int3(-1,-1,1),
            new int3(-1,1,0),
            new int3(-1,1,-1),
            new int3(-1,1,1),
            new int3(1,0,0),
            new int3(1,0,-1),
            new int3(1,0,1),
            new int3(1,-1,0),
            new int3(1,-1,-1),
            new int3(1,-1,1),
            new int3(1,1,0),
            new int3(1,1,-1),
            new int3(1,1,1)
        };

        public static readonly int2[] CARDINAL_LOCALITY =
        {
            new int2(0,0),
            new int2(0,1),
            new int2(1,0),
            new int2(0,-1),
            new int2(-1,0)
        };

        public static readonly int2[] SQUARE_4 =
        {
            new int2(0,0),
            new int2(1,0),
            new int2(0,1),
            new int2(1,1)
        };

        public static readonly int2[] SQUARE_9 =
        {
            new int2(-1,-1),
            new int2(0,-1),
            new int2(1,-1),
            new int2(-1,0),
            new int2(0,0),
            new int2(1,0),
            new int2(-1,1),
            new int2(0,1),
            new int2(1,1)
        };

        public static bool WithinDistance(int2 a, int2 b, int distance)
        {
            long dx = a.x - b.x;
            long dy = a.y - b.y;
            return dx * dx + dy * dy <= (long)distance * distance;
        }
    }
}
