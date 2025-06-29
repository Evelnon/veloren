using System;
using VelorenPort.CoreEngine;
using VelorenPort.NativeMath;

namespace VelorenPort.World.Site.Util;

/// <summary>
/// Gradient utilities used by plot templates. Mirrors the functionality of
/// <c>world/src/site/util/gradient.rs</c>.
/// </summary>
public enum WrapMode
{
    Clamp,
    Repeat,
    PingPong,
}

static class WrapModeExt
{
    public static float Sample(this WrapMode mode, float t)
    {
        return mode switch
        {
            WrapMode.Clamp => math.clamp(t, 0f, 1f),
            WrapMode.Repeat => PositiveFrac(t),
            WrapMode.PingPong => 1f - 2f * math.abs(PositiveFrac(t / 2f) - 0.5f),
            _ => t
        };
    }

    private static float PositiveFrac(float x)
    {
        float f = x - math.floor(x);
        return f >= 0f ? f : f + 1f;
    }
}

/// <summary>Shape for gradient sampling.</summary>
public readonly struct Shape
{
    public enum Kind { Point, Plane, Line }

    public readonly Kind Type;
    public readonly float3 Vec;

    private Shape(Kind type, float3 vec)
    {
        Type = type;
        Vec = math.normalizesafe(vec, new float3(0, 1, 0));
    }

    public static Shape Point => new Shape(Kind.Point, float3.zero);
    public static Shape Plane(float3 normal) => new Shape(Kind.Plane, normal);
    public static Shape Line(float3 dir) => new Shape(Kind.Line, dir);
}

/// <summary>Simple color gradient helper.</summary>
[Serializable]
public struct Gradient
{
    public float3 Center;
    public float Size;
    public Shape Shape;
    public WrapMode Repeat;
    public (Rgb8, Rgb8) Colors;

    public Gradient(float3 center, float size, Shape shape, (Rgb8, Rgb8) colors)
    {
        Center = center;
        Size = size;
        Shape = shape;
        Repeat = WrapMode.Clamp;
        Colors = colors;
    }

    public Gradient WithRepeat(WrapMode mode)
    {
        Repeat = mode;
        return this;
    }

    /// <summary>Sample a color from this gradient.</summary>
    public Rgb8 Sample(float3 pos)
    {
        float dist = Shape.Type switch
        {
            Shape.Kind.Point => math.distance(pos, Center) / Size,
            Shape.Kind.Plane => math.dot(pos - Center, Shape.Vec) / Size,
            Shape.Kind.Line => math.length((pos - Center) - math.dot(pos - Center, Shape.Vec) * Shape.Vec) / Size,
            _ => 0f,
        };
        float t = Repeat.Sample(dist);
        byte r = (byte)math.round(math.lerp(Colors.Item1.R, Colors.Item2.R, t));
        byte g = (byte)math.round(math.lerp(Colors.Item1.G, Colors.Item2.G, t));
        byte b = (byte)math.round(math.lerp(Colors.Item1.B, Colors.Item2.B, t));
        return new Rgb8(r, g, b);
    }
}

