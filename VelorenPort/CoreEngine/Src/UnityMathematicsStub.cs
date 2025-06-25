namespace Unity.Mathematics {
    public struct float2 {
        public float x, y;
        public float2(float x, float y) { this.x = x; this.y = y; }
        public static float2 operator +(float2 a, float2 b) => new float2(a.x + b.x, a.y + b.y);
        public static float2 operator -(float2 a, float2 b) => new float2(a.x - b.x, a.y - b.y);
    }

    public struct float3 {
        public float x, y, z;
        public float3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        public static float3 zero => new float3(0f,0f,0f);
        public static float3 operator +(float3 a, float3 b) => new float3(a.x + b.x, a.y + b.y, a.z + b.z);
        public static float3 operator -(float3 a, float3 b) => new float3(a.x - b.x, a.y - b.y, a.z - b.z);
        public static float3 operator -(float3 v) => new float3(-v.x, -v.y, -v.z);
        public static float3 operator *(float3 a, float3 b) => new float3(a.x * b.x, a.y * b.y, a.z * b.z);
        public static float3 operator *(float3 a, float b) => new float3(a.x * b, a.y * b, a.z * b);
        public static float3 operator *(float b, float3 a) => a * b;
        public static float3 operator +(float3 a, float b) => new float3(a.x + b, a.y + b, a.z + b);
        public static float3 operator +(float b, float3 a) => a + b;
        public static float3 operator -(float3 a, float b) => new float3(a.x - b, a.y - b, a.z - b);
        public static float3 operator /(float3 a, float3 b) => new float3(a.x / b.x, a.y / b.y, a.z / b.z);
        public static float3 operator /(float3 a, float b) => new float3(a.x / b, a.y / b, a.z / b);
        public static float3 operator /(float b, float3 a) => new float3(b / a.x, b / a.y, b / a.z);
        public static bool3 operator >(float3 lhs, float rhs) => new bool3(lhs.x > rhs, lhs.y > rhs, lhs.z > rhs);
        public static bool3 operator <(float3 lhs, float rhs) => new bool3(lhs.x < rhs, lhs.y < rhs, lhs.z < rhs);
    }

    public struct float4 {
        public float x, y, z, w;
        public float4(float x, float y, float z, float w) { this.x = x; this.y = y; this.z = z; this.w = w; }
        public static float4 operator +(float4 a, float4 b) => new float4(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
        public static float4 operator -(float4 a, float4 b) => new float4(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);
        public static float4 operator *(float4 a, float b) => new float4(a.x * b, a.y * b, a.z * b, a.w * b);
        public static float4 operator *(float b, float4 a) => a * b;
        public static float4 operator *(float4 a, float4 b) => new float4(a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w);
        public static float4 operator /(float4 a, float4 b) => new float4(a.x / b.x, a.y / b.y, a.z / b.z, a.w / b.w);
        public static float4 operator /(float4 a, float b) => new float4(a.x / b, a.y / b, a.z / b, a.w / b);
        public static float4 operator -(float4 v) => new float4(-v.x, -v.y, -v.z, -v.w);
        public static float4 operator +(float4 a, float b) => new float4(a.x + b, a.y + b, a.z + b, a.w + b);
        public static float4 operator +(float b, float4 a) => new float4(a.x + b, a.y + b, a.z + b, a.w + b);
        public static float4 operator -(float b, float4 a) => new float4(b - a.x, b - a.y, b - a.z, b - a.w);
    }

    public struct int3 {
        public int x, y, z;
        public int3(int x, int y, int z) { this.x = x; this.y = y; this.z = z; }
        public static explicit operator float3(int3 v) => new float3(v.x, v.y, v.z);
        public static explicit operator int3(float3 v) => new int3((int)v.x, (int)v.y, (int)v.z);
    }

    public struct bool3 {
        public bool x, y, z;
        public bool3(bool x, bool y, bool z) { this.x = x; this.y = y; this.z = z; }
    }

    public struct bool4 {
        public bool x, y, z, w;
        public bool4(bool x, bool y, bool z, bool w) { this.x = x; this.y = y; this.z = z; this.w = w; }
    }

    public struct int2 {
        public int x, y;
        public int2(int x, int y) { this.x = x; this.y = y; }
        public static int2 operator +(int2 a, int2 b) => new int2(a.x + b.x, a.y + b.y);
        public static int2 operator -(int2 a, int2 b) => new int2(a.x - b.x, a.y - b.y);
        public static int2 operator +(int2 a, int b) => new int2(a.x + b, a.y + b);
        public static int2 operator -(int2 a, int b) => new int2(a.x - b, a.y - b);
        public static int2 operator *(int2 a, int b) => new int2(a.x * b, a.y * b);
    }

    public static class math {
        public const float PI = 3.14159265358979323846f;
        public static float floor(float x) => System.MathF.Floor(x);
        public static float3 floor(float3 v) => new float3(floor(v.x), floor(v.y), floor(v.z));
        public static float4 floor(float4 v) => new float4(floor(v.x), floor(v.y), floor(v.z), floor(v.w));
        public static float ceil(float x) => System.MathF.Ceiling(x);
        public static int floorToInt(float x) => (int)System.MathF.Floor(x);
        public static float sqrt(float x) => System.MathF.Sqrt(x);
        public static float sin(float x) => System.MathF.Sin(x);
        public static float cos(float x) => System.MathF.Cos(x);
        public static float abs(float x) => System.MathF.Abs(x);
        public static int abs(int x) => System.Math.Abs(x);
        public static uint max(uint a, uint b) => a > b ? a : b;
        public static float max(float a, float b) => System.MathF.Max(a, b);
        public static float3 max(float3 a, float3 b) => new float3(max(a.x, b.x), max(a.y, b.y), max(a.z, b.z));
        public static float3 abs(float3 v) => new float3(abs(v.x), abs(v.y), abs(v.z));
        public static float4 abs(float4 v) => new float4(abs(v.x), abs(v.y), abs(v.z), abs(v.w));
        public static float length(float3 v) => System.MathF.Sqrt(v.x*v.x + v.y*v.y + v.z*v.z);
        public static float dot(float3 a, float3 b) => a.x*b.x + a.y*b.y + a.z*b.z;
        public static float dot(float4 a, float4 b) => a.x*b.x + a.y*b.y + a.z*b.z + a.w*b.w;
        public static float3 normalize(float3 v) { var l = length(v); return l > 0f ? v * (1f/l) : float3.zero; }
        public static float frac(float x) => x - floor(x);
        public static float3 frac(float3 v) => new float3(frac(v.x), frac(v.y), frac(v.z));
        public static float3 select(float a, float b, bool3 c) => new float3(c.x ? b : a, c.y ? b : a, c.z ? b : a);
        public static float4 select(float a, float b, bool4 c) => new float4(c.x ? b : a, c.y ? b : a, c.z ? b : a, c.w ? b : a);
        public static float cmin(float3 v) => System.MathF.Min(v.x, System.MathF.Min(v.y, v.z));
        public static float3 min(float3 a, float3 b) => new float3(System.MathF.Min(a.x,b.x), System.MathF.Min(a.y,b.y), System.MathF.Min(a.z,b.z));
        public static float4 max(float4 a, float4 b) => new float4(System.MathF.Max(a.x,b.x), System.MathF.Max(a.y,b.y), System.MathF.Max(a.z,b.z), System.MathF.Max(a.w,b.w));
        public static float4 step(float4 y, float4 x) => new float4(x.x >= y.x ? 1f : 0f, x.y >= y.y ? 1f : 0f, x.z >= y.z ? 1f : 0f, x.w >= y.w ? 1f : 0f);
        public static float3 step(float3 y, float3 x) => new float3(x.x >= y.x ? 1f : 0f, x.y >= y.y ? 1f : 0f, x.z >= y.z ? 1f : 0f);
        public static float clamp(float x, float min, float max) => System.MathF.Max(min, System.MathF.Min(max, x));
        public static double fmod(double x, double mod) => x % mod;
        public static float lengthsq(float2 v) => v.x * v.x + v.y * v.y;
    }

    public static class noise {
        private static float mod289(float x) => x - math.floor(x * (1.0f / 289.0f)) * 289.0f;
        private static float3 mod289(float3 x) => x - math.floor(x * (1.0f / 289.0f)) * 289.0f;
        private static float4 mod289(float4 x) => x - math.floor(x * (1.0f / 289.0f)) * 289.0f;
        private static float permute(float x) => mod289((34.0f * x + 1.0f) * x);
        private static float4 permute(float4 x) => mod289((34.0f * x + 1.0f) * x);
        private static float4 taylorInvSqrt(float4 r) => 1.79284291400159f - 0.85373472095314f * r;

        /// <summary>3D simplex noise adapted from Unity.Mathematics.</summary>
        public static float snoise(float3 v) {
            float2 C = new float2(1.0f / 6.0f, 1.0f / 3.0f);
            float4 D = new float4(0.0f, 0.5f, 1.0f, 2.0f);

            float3 i = math.floor(v + math.dot(v, new float3(C.y, C.y, C.y)));
            float3 x0 = v - i + math.dot(i, new float3(C.x, C.x, C.x));

            float3 g = math.step(new float3(x0.y, x0.z, x0.x), x0);
            float3 l = new float3(1f,1f,1f) - g;
            float3 i1 = math.min(g, new float3(l.z, l.x, l.y));
            float3 i2 = math.max(g, new float3(l.z, l.x, l.y));

            float3 x1 = x0 - i1 + new float3(C.x, C.x, C.x);
            float3 x2 = x0 - i2 + new float3(C.y, C.y, C.y);
            float3 x3 = x0 - new float3(D.y, D.y, D.y);

            i = mod289(i);
            float4 p = permute(permute(permute(
                i.z + new float4(0.0f, i1.z, i2.z, 1.0f))
                + i.y + new float4(0.0f, i1.y, i2.y, 1.0f))
                + i.x + new float4(0.0f, i1.x, i2.x, 1.0f));

            float n_ = 0.142857142857f;
            float3 ns = n_ * new float3(D.w, D.y, D.z) - new float3(D.x, D.z, D.x);

            float4 j = p - 49.0f * math.floor(p * ns.z * ns.z);

            float4 x_ = math.floor(j * ns.z);
            float4 y_ = math.floor(j - 7.0f * x_);

            float4 x = x_ * ns.x + new float4(ns.y, ns.y, ns.y, ns.y);
            float4 y = y_ * ns.x + new float4(ns.y, ns.y, ns.y, ns.y);
            float4 h = new float4(1.0f,1.0f,1.0f,1.0f) - math.abs(x) - math.abs(y);

            float4 b0 = new float4(x.x, x.y, y.x, y.y);
            float4 b1 = new float4(x.z, x.w, y.z, y.w);

            float4 s0 = math.floor(b0) * 2.0f + new float4(1.0f,1.0f,1.0f,1.0f);
            float4 s1 = math.floor(b1) * 2.0f + new float4(1.0f,1.0f,1.0f,1.0f);
            float4 sh = -math.step(h, new float4(0f,0f,0f,0f));

            float4 a0 = new float4(b0.x, b0.z, b0.y, b0.w) + new float4(s0.x, s0.z, s0.y, s0.w) * new float4(sh.x, sh.x, sh.y, sh.y);
            float4 a1 = new float4(b1.x, b1.z, b1.y, b1.w) + new float4(s1.x, s1.z, s1.y, s1.w) * new float4(sh.z, sh.z, sh.w, sh.w);

            float3 p0 = new float3(a0.x, a0.y, h.x);
            float3 p1 = new float3(a0.z, a0.w, h.y);
            float3 p2 = new float3(a1.x, a1.y, h.z);
            float3 p3 = new float3(a1.z, a1.w, h.w);

            float4 norm = taylorInvSqrt(new float4(math.dot(p0, p0), math.dot(p1, p1), math.dot(p2, p2), math.dot(p3, p3)));
            p0 *= norm.x; p1 *= norm.y; p2 *= norm.z; p3 *= norm.w;

            float4 m = math.max(new float4(0.6f,0.6f,0.6f,0.6f) - new float4(math.dot(x0, x0), math.dot(x1, x1), math.dot(x2, x2), math.dot(x3, x3)), new float4(0f,0f,0f,0f));
            m = m * m;
            return 42.0f * math.dot(m * m, new float4(math.dot(p0, x0), math.dot(p1, x1), math.dot(p2, x2), math.dot(p3, x3)));
        }
    }

    public struct Random {
        private System.Random _rng;
        public Random(uint seed) { _rng = new System.Random((int)seed); }
        public float3 NextFloat3() => new float3((float)_rng.NextDouble(), (float)_rng.NextDouble(), (float)_rng.NextDouble());
    }
}
