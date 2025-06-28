using System;
using Unity.Mathematics;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Generic 3D volume container storing values in a flat array.
    /// </summary>
    [Serializable]
    public class Vol<T> where T : struct {
        private readonly int3 _size;
        private readonly T[] _data;

        public int3 Size => _size;

        public Vol(int3 size) {
            _size = size;
            _data = new T[size.x * size.y * size.z];
        }

        public T this[int x, int y, int z] {
            get => _data[Index(x, y, z)];
            set => _data[Index(x, y, z)] = value;
        }

        public T this[int3 pos] {
            get => this[pos.x, pos.y, pos.z];
            set => this[pos.x, pos.y, pos.z] = value;
        }

        public void Fill(T value) {
            for (int i = 0; i < _data.Length; i++) _data[i] = value;
        }

        public System.Collections.Generic.IEnumerable<(int3 Pos, T Value)> Cells() {
            for (int z = 0; z < _size.z; z++)
            for (int y = 0; y < _size.y; y++)
            for (int x = 0; x < _size.x; x++)
                yield return (new int3(x, y, z), this[x,y,z]);
        }

        public bool InBounds(int x, int y, int z) =>
            x >= 0 && y >= 0 && z >= 0 &&
            x < _size.x && y < _size.y && z < _size.z;

        public Vol<T> Clone() {
            var copy = new Vol<T>(_size);
            Array.Copy(_data, copy._data, _data.Length);
            return copy;
        }

        public void CopyTo(Vol<T> dst) {
            if (!dst._size.Equals(_size))
                throw new ArgumentException("Volume sizes must match");
            Array.Copy(_data, dst._data, _data.Length);
        }

        public System.Collections.Generic.IEnumerator<(int3 Pos, T Value)> GetEnumerator() => Cells().GetEnumerator();

        private int Index(int x, int y, int z) => x + _size.x * (y + _size.y * z);
    }
}
