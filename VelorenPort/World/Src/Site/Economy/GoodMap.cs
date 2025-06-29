using System;
using System.Collections.Generic;

namespace VelorenPort.World.Site.Economy {
    /// <summary>
    /// Fixed-size map keyed by <see cref="GoodIndex"/>. Mirrors <c>GoodMap</c>
    /// from the Rust implementation.
    /// </summary>
    [Serializable]
    public class GoodMap<T> where T : struct {
        private readonly T[] _data = new T[GoodIndex.LENGTH];

        public T this[GoodIndex idx] {
            get => _data[idx.ToInt()];
            set => _data[idx.ToInt()] = value;
        }

        public static GoodMap<T> FromDefault(T value) {
            var map = new GoodMap<T>();
            for (int i = 0; i < GoodIndex.LENGTH; i++) map._data[i] = value;
            return map;
        }

        public static GoodMap<T> FromIter(IEnumerable<(GoodIndex, T)> entries, T defaultValue) {
            var map = FromDefault(defaultValue);
            foreach (var (idx, val) in entries)
                map._data[idx.ToInt()] = val;
            return map;
        }

        public static GoodMap<T> FromList(IEnumerable<(GoodIndex, T)> entries, T defaultValue) =>
            FromIter(entries, defaultValue);

        public GoodMap<U> Map<U>(Func<GoodIndex, T, U> f) where U : struct {
            var result = new GoodMap<U>();
            for (int i = 0; i < GoodIndex.LENGTH; i++)
                result._data[i] = f(new GoodIndex(i), _data[i]);
            return result;
        }

        public IEnumerable<(GoodIndex, T)> Iterate() {
            for (int i = 0; i < GoodIndex.LENGTH; i++) yield return (new GoodIndex(i), _data[i]);
        }
    }
}
