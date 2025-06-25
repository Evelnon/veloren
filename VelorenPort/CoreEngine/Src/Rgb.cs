using System;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Simple RGB triple generic over component type. Mirrors vek::Rgb.
    /// </summary>
    [Serializable]
    public struct Rgb<T> {
        public T R;
        public T G;
        public T B;

        public Rgb(T r, T g, T b) {
            R = r;
            G = g;
            B = b;
        }

        /// <summary>Map each component using the provided function.</summary>
        public Rgb<U> Map<U>(Func<T, U> f) => new Rgb<U>(f(R), f(G), f(B));
    }
}
