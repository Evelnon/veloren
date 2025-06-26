using System;
using Unity.Mathematics;

namespace VelorenPort.World {
    /// <summary>
    /// Dimensions and helper methods for terrain chunks.
    /// Mirrors <c>TerrainChunkSize</c> in the Rust code.
    /// </summary>
    public static class TerrainChunkSize {
        /// <summary>Base-two logarithm of the number of blocks per axis.</summary>
        public const uint TERRAIN_CHUNK_BLOCKS_LG = 5;

        /// <summary>Size of a chunk in blocks.</summary>
        public static readonly int2 RectSize = new int2(1 << (int)TERRAIN_CHUNK_BLOCKS_LG,
                                                         1 << (int)TERRAIN_CHUNK_BLOCKS_LG);

        private static int DivEuclid(int a, int b)
        {
            int q = a / b;
            int r = a % b;
            if ((r > 0 && b < 0) || (r < 0 && b > 0)) q -= 1;
            return q;
        }

        /// <summary>
        /// Convert chunk dimensions into block dimensions.
        /// </summary>
        public static int2 Blocks(int2 chunks) => chunks * RectSize;

        /// <summary>
        /// World position at the center of the given chunk.
        /// </summary>
        public static int2 CenterWpos(int2 chunkPos) => chunkPos * RectSize + RectSize / 2;

        /// <summary>
        /// Convert world block coordinates to chunk coordinates using Euclidean division.
        /// </summary>
        public static int2 WposToCpos(int2 wpos) =>
            new int2(DivEuclid(wpos.x, RectSize.x), DivEuclid(wpos.y, RectSize.y));

        /// <summary>Convert chunk coordinates to world block coordinates.</summary>
        public static int2 CposToWpos(int2 cpos) => cpos * RectSize;

        /// <summary>World position at the center of the specified chunk coordinate.</summary>
        public static int2 CposToWposCenter(int2 cpos) => cpos * RectSize + RectSize / 2;

        /// <summary>Convert world block coordinates to chunk coordinates (float).</summary>
        public static float2 WposToCpos(float2 wpos) => wpos / (float2)RectSize;

        /// <summary>Convert chunk coordinates to world block coordinates (float).</summary>
        public static float2 CposToWpos(float2 cpos) => cpos * (float2)RectSize;

        /// <summary>World position at the center of the specified chunk coordinate (float).</summary>
        public static float2 CposToWposCenter(float2 cpos) => cpos * (float2)RectSize + (float2)RectSize / 2f;

        /// <summary>Convert world block coordinates to chunk coordinates (double).</summary>
        public static double2 WposToCpos(double2 wpos) => wpos / (double2)RectSize;

        /// <summary>Convert chunk coordinates to world block coordinates (double).</summary>
        public static double2 CposToWpos(double2 cpos) => cpos * (double2)RectSize;

        /// <summary>World position at the center of the specified chunk coordinate (double).</summary>
        public static double2 CposToWposCenter(double2 cpos) => cpos * (double2)RectSize + (double2)RectSize / 2d;
    }
}
