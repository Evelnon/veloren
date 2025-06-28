using System;

namespace VelorenPort.CoreEngine
{
    /// <summary>
    /// Generic RGBA color structure mirroring vek::Rgba.
    /// </summary>
    [Serializable]
    public struct Rgba<T>
    {
        public T R;
        public T G;
        public T B;
        public T A;

        public Rgba(T r, T g, T b, T a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        /// <summary>Create a color from RGB and alpha components.</summary>
        public static Rgba<T> FromTranslucent(Rgb<T> rgb, T a) => new(rgb.R, rgb.G, rgb.B, a);

        /// <summary>Map all components using the provided function.</summary>
        public Rgba<U> Map<U>(Func<T, U> f) => new(f(R), f(G), f(B), f(A));

        public static implicit operator Rgb<T>(Rgba<T> rgba) => new(rgba.R, rgba.G, rgba.B);
    }
}
