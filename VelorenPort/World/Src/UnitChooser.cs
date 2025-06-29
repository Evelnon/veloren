using System;
using VelorenPort.NativeMath;
using VelorenPort.CoreEngine;

namespace VelorenPort.World
{
    /// <summary>
    /// Deterministic selector of perpendicular unit vectors. Port of
    /// <c>UnitChooser</c> from the Rust project.
    /// </summary>
    [Serializable]
    public struct UnitChooser
    {
        private readonly RandomPerm _perm;

        private static readonly (int2, int2)[] Choices =
        {
            (new int2(1, 0), new int2(0, 1)),
            (new int2(1, 0), new int2(0, -1)),
            (new int2(-1, 0), new int2(0, 1)),
            (new int2(-1, 0), new int2(0, -1)),
            (new int2(0, 1), new int2(1, 0)),
            (new int2(0, 1), new int2(-1, 0)),
            (new int2(0, -1), new int2(1, 0)),
            (new int2(0, -1), new int2(-1, 0)),
        };

        public UnitChooser(uint seed)
        {
            _perm = new RandomPerm(seed);
        }

        /// <summary>
        /// Get a pair of unit vectors for the given index.
        /// </summary>
        public (int2, int2) Get(uint index)
        {
            uint val = _perm.Get(index);
            return Choices[val % (uint)Choices.Length];
        }
    }
}
