using System;
using VelorenPort.NativeMath;

namespace VelorenPort.CoreEngine
{
    /// <summary>
    /// Utility conversions between linear and srgb color spaces and related helpers.
    /// Ported from common/src/util/color.rs.
    /// </summary>
    public static class ColorUtil
    {
        public static Rgb<float> SrgbToLinearFast(Rgb<float> col)
        {
            return col.Map(c => c <= 0.104f
                ? c * 0.08677088f
                : 0.012522878f * c + 0.682171111f * c * c + 0.305306011f * c * c * c);
        }

        public static Rgb<float> SrgbToLinear(Rgb<float> col)
        {
            return col.Map(c => c <= 0.04045f
                ? c / 12.92f
                : math.pow((c + 0.055f) / 1.055f, 2.4f));
        }

        public static Rgb<float> LinearToSrgb(Rgb<float> col)
        {
            return col.Map(c => c <= 0.0031308f
                ? c * 12.92f
                : 1.055f * math.pow(c, 1f / 2.4f) - 0.055f);
        }

        public static Rgba<float> SrgbaToLinear(Rgba<float> col) =>
            Rgba<float>.FromTranslucent(SrgbToLinearFast(col), col.A);

        public static Rgba<float> LinearToSrgba(Rgba<float> col) =>
            Rgba<float>.FromTranslucent(LinearToSrgb(col), col.A);

        public static float3 RgbToHsv(Rgb<float> rgb)
        {
            float r = rgb.R; float g = rgb.G; float b = rgb.B;
            float max, min, diff, add;
            if (r > g)
            {
                max = r; min = g; diff = g - b; add = 0f;
            }
            else
            {
                max = g; min = r; diff = b - r; add = 2f;
            }
            if (b > max)
            {
                max = b;
                min = math.min(min, g);
                diff = r - g;
                add = 4f;
            }
            else
            {
                min = math.min(min, b);
            }
            float v = max;
            float h;
            if (max == min)
            {
                h = 0f;
            }
            else
            {
                h = 60f * (add + diff / (max - min));
                if (h < 0f) h += 360f;
            }
            float s = max == 0f ? 0f : (max - min) / max;
            return new float3(h, s, v);
        }

        public static Rgb<float> HsvToRgb(float3 hsv)
        {
            float h = hsv.x; float s = hsv.y; float v = hsv.z;
            float c = s * v;
            float hp = h / 60f;
            float x = c * (1f - math.abs(hp % 2f - 1f));
            float m = v - c;
            float3 rgb = hp switch
            {
                >= 0f and <= 1f => new float3(c, x, 0f),
                > 1f and <= 2f => new float3(x, c, 0f),
                > 2f and <= 3f => new float3(0f, c, x),
                > 3f and <= 4f => new float3(0f, x, c),
                > 4f and <= 5f => new float3(x, 0f, c),
                _ => new float3(c, 0f, x)
            };
            rgb += new float3(m, m, m);
            return new Rgb<float>(rgb.x, rgb.y, rgb.z);
        }

        public static float3 RgbToXyz(Rgb<float> rgb)
        {
            float3 v = new(rgb.R, rgb.G, rgb.B);
            float3 x = new float3(
                0.4124f, 0.2126f, 0.0193f);
            float3 y = new float3(
                0.3576f, 0.7152f, 0.1192f);
            float3 z = new float3(
                0.1805f, 0.0722f, 0.9504f);
            return new float3(math.dot(x, v), math.dot(y, v), math.dot(z, v));
        }

        public static float3 RgbToXyy(Rgb<float> rgb)
        {
            float3 xyz = RgbToXyz(rgb);
            float sum = xyz.x + xyz.y + xyz.z;
            return new float3(xyz.x / sum, xyz.y / sum, xyz.y);
        }

        public static Rgb<float> XyyToRgb(float3 xyy)
        {
            float3 xyz = new(xyy.z / xyy.y * xyy.x,
                              xyy.z,
                              xyy.z / xyy.y * (1f - xyy.x - xyy.y));
            float3 r = new float3(
                3.2406f, -0.9689f, 0.0557f);
            float3 g = new float3(
                -1.5372f, 1.8758f, -0.2040f);
            float3 b = new float3(
                -0.4986f, 0.0415f, 1.0570f);
            return new Rgb<float>(math.dot(r, xyz), math.dot(g, xyz), math.dot(b, xyz));
        }

        public static Rgb<float> SaturateSrgb(Rgb<float> col, float value)
        {
            float3 hsv = RgbToHsv(SrgbToLinearFast(col));
            hsv.y *= 1f + value;
            Rgb<float> rgbCol = HsvToRgb(hsv);
            float3 rgb = new float3(rgbCol.R, rgbCol.G, rgbCol.B);
            rgb = math.clamp(rgb, new float3(0f, 0f, 0f), new float3(1f, 1f, 1f));
            return LinearToSrgb(new Rgb<float>(rgb.x, rgb.y, rgb.z));
        }

        public static Rgb<float> ChromifySrgb(Rgb<float> luma, Rgb<float> chroma)
        {
            float l = RgbToXyy(SrgbToLinearFast(luma)).z;
            float3 xyy = RgbToXyy(SrgbToLinearFast(chroma));
            xyy.z = l;
            var rgb = XyyToRgb(xyy).Map(e => math.clamp(e, 0f, 1f));
            return LinearToSrgb(rgb);
        }
    }
}
