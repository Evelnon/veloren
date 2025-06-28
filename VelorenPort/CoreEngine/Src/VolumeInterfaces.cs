using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Base readonly access to a 3D volume.
    /// </summary>
    public interface IReadVol<T> {
        T Get(int3 pos);
        bool InBounds(int3 pos);
    }

    /// <summary>
    /// Write access to a 3D volume.
    /// </summary>
    public interface IWriteVol<T> : IReadVol<T> {
        void Set(int3 pos, T value);
        T Map(int3 pos, Func<T, T> f);
    }

    /// <summary>
    /// Volume with explicit integer bounds.
    /// </summary>
    public interface ISizedVol {
        int3 LowerBound { get; }
        int3 UpperBound { get; }
        int3 Size { get; }
    }

    /// <summary>
    /// Utility enumerator yielding positions within bounds.
    /// </summary>
    public struct DefaultPosEnumerator : IEnumerable<int3> {
        private readonly int3 _min;
        private readonly int3 _max;

        public DefaultPosEnumerator(int3 min, int3 max) {
            _min = min;
            _max = max;
        }

        public IEnumerator<int3> GetEnumerator() {
            for (int z = _min.z; z < _max.z; z++)
            for (int y = _min.y; y < _max.y; y++)
            for (int x = _min.x; x < _max.x; x++)
                yield return new int3(x, y, z);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>
    /// Enumerator that yields position and voxel pairs for a volume.
    /// </summary>
    public struct DefaultVolEnumerator<T> : IEnumerable<(int3 Pos, T Vox)> {
        private readonly IReadVol<T> _vol;
        private readonly int3 _min;
        private readonly int3 _max;

        public DefaultVolEnumerator(IReadVol<T> vol, int3 min, int3 max) {
            _vol = vol;
            _min = min;
            _max = max;
        }

        public IEnumerator<(int3 Pos, T Vox)> GetEnumerator() {
            foreach (var p in new DefaultPosEnumerator(_min, _max))
                if (_vol.InBounds(p))
                    yield return (p, _vol.Get(p));
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
