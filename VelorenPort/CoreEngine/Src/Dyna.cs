using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace VelorenPort.CoreEngine
{
    /// <summary>
    /// Dynamic 3D volume similar to common::volumes::dyna::Dyna in Rust.
    /// Stores voxels in column-major order and keeps generic metadata.
    /// </summary>
    /// <remarks>
    /// This is a partial port focusing on basic storage and access.
    /// </remarks>
    [Serializable]
    public class Dyna<V,M> : IWriteVol<V>, ISizedVol, IEnumerable<(int3 Pos, V Vox)>
    {
        private readonly V[] _vox;
        private readonly int3 _size;
        private M _meta;

        public Dyna(int3 size, V fill, M meta)
        {
            _size = size;
            _meta = meta;
            _vox = new V[size.x * size.y * size.z];
            for (int i = 0; i < _vox.Length; i++)
                _vox[i] = fill;
        }

        /// <summary>
        /// Convenience constructor mirroring the Rust <c>filled</c> helper.
        /// </summary>
        public static Dyna<V,M> Filled(int3 size, V fill, M meta) =>
            new Dyna<V,M>(size, fill, meta);

        /// <summary>
        /// Create a new volume using a function to generate each voxel.
        /// </summary>
        public static Dyna<V,M> FromFunc(int3 size, M meta, Func<int3, V> f)
        {
            var d = new Dyna<V,M>(size, default!, meta);
            for (int x = 0; x < size.x; x++)
            for (int y = 0; y < size.y; y++)
            for (int z = 0; z < size.z; z++)
                d[new int3(x, y, z)] = f(new int3(x, y, z));
            return d;
        }

        public int3 Size => _size;
        public int3 LowerBound => int3.zero;
        public int3 UpperBound => _size;
        public M Metadata { get => _meta; set => _meta = value; }

        private int Index(int3 pos)
        {
            if (pos.x < 0 || pos.y < 0 || pos.z < 0 ||
                pos.x >= _size.x || pos.y >= _size.y || pos.z >= _size.z)
                throw new ArgumentOutOfRangeException(nameof(pos));
            return pos.x * _size.y * _size.z + pos.y * _size.z + pos.z;
        }

        public V this[int3 pos]
        {
            get => _vox[Index(pos)];
            set => _vox[Index(pos)] = value;
        }

        public bool InBounds(int3 pos) =>
            pos.x >= 0 && pos.y >= 0 && pos.z >= 0 &&
            pos.x < _size.x && pos.y < _size.y && pos.z < _size.z;

        public V Get(int3 pos) => this[pos];

        public void Set(int3 pos, V value) => this[pos] = value;

        public V Map(int3 pos, Func<V, V> f)
        {
            var old = this[pos];
            var nw = f(old);
            this[pos] = nw;
            return nw;
        }

        public void Fill(V value)
        {
            for (int i = 0; i < _vox.Length; i++)
                _vox[i] = value;
        }

        public Dyna<V,M> Clone()
        {
            var copy = new Dyna<V,M>(_size, default!, _meta);
            Array.Copy(_vox, copy._vox, _vox.Length);
            return copy;
        }

        public void CopyTo(Dyna<V,M> dst)
        {
            if (!dst._size.Equals(_size))
                throw new ArgumentException("Volume sizes must match");
            Array.Copy(_vox, dst._vox, _vox.Length);
        }

        /// <summary>
        /// Enumerate all valid positions within the volume bounds.
        /// </summary>
        public IEnumerable<int3> Positions()
        {
            for (int x = 0; x < _size.x; x++)
            for (int y = 0; y < _size.y; y++)
            for (int z = 0; z < _size.z; z++)
                yield return new int3(x, y, z);
        }

        public IEnumerable<(int3 Pos, V Vox)> Cells()
        {
            for (int x = 0; x < _size.x; x++)
            for (int y = 0; y < _size.y; y++)
            for (int z = 0; z < _size.z; z++)
                yield return (new int3(x, y, z), _vox[Index(new int3(x, y, z))]);
        }

        public IEnumerator<(int3 Pos, V Vox)> GetEnumerator() => Cells().GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        public Dyna<W,M> MapInto<W>(Func<V,W> f)
        {
            var res = new Dyna<W,M>(_size, default!, _meta);
            for (int i = 0; i < _vox.Length; i++)
                res._vox[i] = f(_vox[i]);
            return res;
        }
    }
}
