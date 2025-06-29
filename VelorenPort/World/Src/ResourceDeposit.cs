using System;
using VelorenPort.NativeMath;

namespace VelorenPort.World
{
    /// <summary>
    /// Information about a resource block generated within a chunk.
    /// </summary>
    [Serializable]
    public struct ResourceDeposit
    {
        public int3 Position { get; set; }
        public BlockKind Kind { get; set; }

        public ResourceDeposit(int3 position, BlockKind kind)
        {
            Position = position;
            Kind = kind;
        }
    }
}
