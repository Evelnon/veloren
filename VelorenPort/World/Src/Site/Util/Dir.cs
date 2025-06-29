using System;
using VelorenPort.NativeMath;
using VelorenPort.CoreEngine;

namespace VelorenPort.World.Site.Util
{
    /// <summary>
    /// Two-dimensional cardinal directions used for town generation.
    /// Ported in simplified form from the Rust implementation.
    /// </summary>
    public enum Dir
    {
        X,
        Y,
        NegX,
        NegY,
    }

    /// <summary>Utility helpers for <see cref="Dir"/>.</summary>
    public static class DirExt
    {
        /// <summary>Select a random direction.</summary>
        public static Dir Choose(System.Random rng)
            => (Dir)rng.Next(0, 4);

        /// <summary>Create a direction based on a vector.</summary>
        public static Dir FromVec2(int2 vec)
        {
            return math.abs(vec.x) > math.abs(vec.y)
                ? (vec.x > 0 ? Dir.X : Dir.NegX)
                : (vec.y > 0 ? Dir.Y : Dir.NegY);
        }

        public static Dir Opposite(this Dir dir)
            => dir switch
            {
                Dir.X => Dir.NegX,
                Dir.NegX => Dir.X,
                Dir.Y => Dir.NegY,
                _ => Dir.Y,
            };

        public static Dir RotatedCcw(this Dir dir)
            => dir switch
            {
                Dir.X => Dir.Y,
                Dir.Y => Dir.NegX,
                Dir.NegX => Dir.NegY,
                _ => Dir.X,
            };

        public static Dir RotatedCw(this Dir dir) => dir.RotatedCcw().Opposite();

        public static Dir Orthogonal(this Dir dir)
            => dir is Dir.X or Dir.NegX ? Dir.Y : Dir.X;

        public static Dir Abs(this Dir dir)
            => dir is Dir.X or Dir.NegX ? Dir.X : Dir.Y;

        public static int Signum(this Dir dir)
            => dir is Dir.X or Dir.Y ? 1 : -1;

        public static bool IsX(this Dir dir) => dir is Dir.X or Dir.NegX;
        public static bool IsY(this Dir dir) => dir is Dir.Y or Dir.NegY;
        public static bool IsPositive(this Dir dir) => dir is Dir.X or Dir.Y;
        public static bool IsNegative(this Dir dir) => !dir.IsPositive();

        public static Dir RelativeTo(this Dir dir, Dir other)
            => other switch
            {
                Dir.X => dir,
                Dir.NegX => dir.Opposite(),
                Dir.Y => dir.RotatedCw(),
                _ => dir.RotatedCcw(),
            };

        public static int2 ToVec2(this Dir dir)
            => dir switch
            {
                Dir.X => new int2(1, 0),
                Dir.NegX => new int2(-1, 0),
                Dir.Y => new int2(0, 1),
                _ => new int2(0, -1),
            };

        /// <summary>Diagonal vector to the left of this direction.</summary>
        public static int2 Diagonal(this Dir dir) => dir.ToVec2() + dir.RotatedCcw().ToVec2();

        public static int3 ToVec3(this Dir dir)
            => dir switch
            {
                Dir.X => new int3(1, 0, 0),
                Dir.NegX => new int3(-1, 0, 0),
                Dir.Y => new int3(0, 1, 0),
                _ => new int3(0, -1, 0),
            };

        public static int2 Vec2(this Dir dir, int x, int y)
            => dir switch
            {
                Dir.X => new int2(x, y),
                Dir.NegX => new int2(-x, -y),
                Dir.Y => new int2(y, x),
                _ => new int2(-y, -x),
            };

        public static int2 Vec2Abs(this Dir dir, int x, int y)
            => dir switch
            {
                Dir.X => new int2(x, y),
                Dir.NegX => new int2(x, y),
                Dir.Y => new int2(y, x),
                _ => new int2(y, x),
            };

        public static float3x3 ToMat3(this Dir dir) => dir switch
        {
            Dir.X => float3x3.identity,
            Dir.NegX => new float3x3(-1, 0, 0,
                                      0, -1, 0,
                                      0, 0, 1),
            Dir.Y => new float3x3(0, -1, 0,
                                  1, 0, 0,
                                  0, 0, 1),
            _ => new float3x3(0, 1, 0,
                              -1, 0, 0,
                               0, 0, 1),
        };

        public static float3x3 FromZMat3(this Dir dir) => dir switch
        {
            Dir.X => new float3x3(0, 0, -1,
                                  0, 1, 0,
                                  1, 0, 0),
            Dir.NegX => new float3x3(0, 0, 1,
                                     0, 1, 0,
                                    -1, 0, 0),
            Dir.Y => new float3x3(1, 0, 0,
                                  0, 0, -1,
                                  0, 1, 0),
            _ => new float3x3(1, 0, 0,
                             0, 0, 1,
                             0, -1, 0),
        };

        public static int Select(this Dir dir, int2 vec)
            => dir is Dir.X or Dir.NegX ? vec.x : vec.y;

        public static int2 SelectWith(this Dir dir, int2 vec, int2 other)
            => dir is Dir.X or Dir.NegX
                ? new int2(vec.x, other.y)
                : new int2(other.x, vec.y);

        public static int SpriteOri(this Dir dir) => dir switch
        {
            Dir.X => 0,
            Dir.Y => 2,
            Dir.NegX => 4,
            _ => 6,
        };

        public static (Aabr first, Aabr second) SplitAabrOffset(this Dir dir, Aabr aabr, int offset)
        {
            return dir switch
            {
                Dir.X =>
                    (new Aabr(aabr.Min, new int2(aabr.Min.x + offset, aabr.Max.y)),
                     new Aabr(new int2(aabr.Min.x + offset, aabr.Min.y), aabr.Max)),
                Dir.Y =>
                    (new Aabr(aabr.Min, new int2(aabr.Max.x, aabr.Min.y + offset)),
                     new Aabr(new int2(aabr.Min.x, aabr.Min.y + offset), aabr.Max)),
                Dir.NegX =>
                    (new Aabr(new int2(aabr.Max.x - offset, aabr.Min.y), aabr.Max),
                     new Aabr(aabr.Min, new int2(aabr.Max.x - offset, aabr.Max.y))),
                _ =>
                    (new Aabr(new int2(aabr.Min.x, aabr.Max.y - offset), aabr.Max),
                     new Aabr(aabr.Min, new int2(aabr.Max.x, aabr.Max.y - offset))),
            };
        }

        public static Aabr ExtendAabr(this Dir dir, Aabr aabr, int amount)
        {
            int2 offset = dir.ToVec2() * amount;
            return dir.IsPositive()
                ? new Aabr(aabr.Min, aabr.Max + offset)
                : new Aabr(aabr.Min + offset, aabr.Max);
        }

        public static Aabr TrimAabr(this Dir dir, Aabr aabr, int amount)
            => dir.Opposite().ExtendAabr(aabr, -amount);

        public static (Dir dir, int rest)? FromSpriteOri(int ori)
        {
            Dir dir = ori / 2 switch
            {
                0 => Dir.X,
                1 => Dir.Y,
                2 => Dir.NegX,
                3 => Dir.NegY,
                _ => (Dir)(-1)
            };
            if ((int)dir == -1) return null;
            return (dir, ori % 2);
        }

        public static int SpriteOriLegacy(this Dir dir) => dir switch
        {
            Dir.X => 2,
            Dir.NegX => 6,
            Dir.Y => 4,
            _ => 0,
        };

        public static int SelectAabr(this Dir dir, Aabr aabr) => dir switch
        {
            Dir.X => aabr.Max.x,
            Dir.NegX => aabr.Min.x,
            Dir.Y => aabr.Max.y,
            _ => aabr.Min.y,
        };

        public static int2 SelectAabrWith(this Dir dir, Aabr aabr, int2 other) => dir switch
        {
            Dir.X => new int2(aabr.Max.x, other.y),
            Dir.NegX => new int2(aabr.Min.x, other.y),
            Dir.Y => new int2(other.x, aabr.Max.y),
            _ => new int2(other.x, aabr.Min.y),
        };

        public static (Aabr first, Aabr second) SplitAabrOffset(this Dir dir, Aabr aabr, int offset)
        {
            return dir switch
            {
                Dir.X => (new Aabr(aabr.Min, new int2(aabr.Min.x + offset, aabr.Max.y)),
                            new Aabr(new int2(aabr.Min.x + offset, aabr.Min.y), aabr.Max)),
                Dir.Y => (new Aabr(aabr.Min, new int2(aabr.Max.x, aabr.Min.y + offset)),
                            new Aabr(new int2(aabr.Min.x, aabr.Min.y + offset), aabr.Max)),
                Dir.NegX =>
                {
                    var res = Dir.X.SplitAabrOffset(aabr, aabr.Max.x - offset);
                    return (res.second, res.first);
                },
                _ =>
                {
                    var res = Dir.Y.SplitAabrOffset(aabr, aabr.Max.y - offset);
                    return (res.second, res.first);
                }
            };
        }

        public static Aabr ExtendAabr(this Dir dir, Aabr aabr, int amount)
        {
            int2 off = dir.ToVec2() * amount;
            return dir.IsPositive()
                ? new Aabr(aabr.Min, aabr.Max + off)
                : new Aabr(aabr.Min + off, aabr.Max);
        }

        public static Aabr TrimAabr(this Dir dir, Aabr aabr, int amount)
            => (-dir).ExtendAabr(aabr, -amount);
    }

    /// <summary>Three-dimensional directions.</summary>
    public enum Dir3
    {
        X,
        Y,
        Z,
        NegX,
        NegY,
        NegZ,
    }

    /// <summary>Utility helpers for <see cref="Dir3"/>.</summary>
    public static class Dir3Ext
    {
        public static Dir3 Choose(System.Random rng)
            => (Dir3)rng.Next(0, 6);

        public static Dir3 FromDir(Dir dir)
            => dir switch
            {
                Dir.X => Dir3.X,
                Dir.Y => Dir3.Y,
                Dir.NegX => Dir3.NegX,
                _ => Dir3.NegY,
            };

        public static Dir? ToDir(this Dir3 dir)
            => dir switch
            {
                Dir3.X => Dir.X,
                Dir3.Y => Dir.Y,
                Dir3.NegX => Dir.NegX,
                Dir3.NegY => Dir.NegY,
                _ => null,
            };

        public static Dir3 Opposite(this Dir3 dir)
            => dir switch
            {
                Dir3.X => Dir3.NegX,
                Dir3.NegX => Dir3.X,
                Dir3.Y => Dir3.NegY,
                Dir3.NegY => Dir3.Y,
                Dir3.Z => Dir3.NegZ,
                _ => Dir3.Z,
            };

        public static int Signum(this Dir3 dir)
            => dir is Dir3.X or Dir3.Y or Dir3.Z ? 1 : -1;

        public static bool IsX(this Dir3 dir) => dir is Dir3.X or Dir3.NegX;
        public static bool IsY(this Dir3 dir) => dir is Dir3.Y or Dir3.NegY;
        public static bool IsZ(this Dir3 dir) => dir is Dir3.Z or Dir3.NegZ;
        public static bool IsPositive(this Dir3 dir) => dir is Dir3.X or Dir3.Y or Dir3.Z;
        public static bool IsNegative(this Dir3 dir) => !dir.IsPositive();

        public static Dir3 FromVec3(int3 vec)
        {
            int ax = math.abs(vec.x);
            int ay = math.abs(vec.y);
            int az = math.abs(vec.z);
            if (ax >= ay && ax >= az) return vec.x >= 0 ? Dir3.X : Dir3.NegX;
            if (ay >= az) return vec.y >= 0 ? Dir3.Y : Dir3.NegY;
            return vec.z >= 0 ? Dir3.Z : Dir3.NegZ;
        }

        public static int3 ToVec3(this Dir3 dir)
            => dir switch
            {
                Dir3.X => new int3(1, 0, 0),
                Dir3.NegX => new int3(-1, 0, 0),
                Dir3.Y => new int3(0, 1, 0),
                Dir3.NegY => new int3(0, -1, 0),
                Dir3.Z => new int3(0, 0, 1),
                _ => new int3(0, 0, -1),
            };

        public static Dir3 RotateAxisCcw(this Dir3 dir, Dir3 axis) => axis switch
        {
            Dir3.X or Dir3.NegX => dir switch
            {
                Dir3.Y => Dir3.Z,
                Dir3.NegY => Dir3.NegZ,
                Dir3.Z => Dir3.NegY,
                Dir3.NegZ => Dir3.Y,
                _ => dir,
            },
            Dir3.Y or Dir3.NegY => dir switch
            {
                Dir3.X => Dir3.Z,
                Dir3.NegX => Dir3.NegZ,
                Dir3.Z => Dir3.NegX,
                Dir3.NegZ => Dir3.X,
                _ => dir,
            },
            _ => dir switch
            {
                Dir3.X => Dir3.Y,
                Dir3.NegX => Dir3.NegY,
                Dir3.Y => Dir3.NegX,
                Dir3.NegY => Dir3.X,
                _ => dir,
            }
        };

        public static Dir3 RotateAxisCw(this Dir3 dir, Dir3 axis)
            => dir.RotateAxisCcw(axis).Opposite();

        public static Dir3 Cross(this Dir3 a, Dir3 b) => (a, b) switch
        {
            (Dir3.X or Dir3.NegX, Dir3.Y or Dir3.NegY)
                or (Dir3.Y or Dir3.NegY, Dir3.X or Dir3.NegX) => Dir3.Z,
            (Dir3.X or Dir3.NegX, Dir3.Z or Dir3.NegZ)
                or (Dir3.Z or Dir3.NegZ, Dir3.X or Dir3.NegX) => Dir3.Y,
            (Dir3.Z or Dir3.NegZ, Dir3.Y or Dir3.NegY)
                or (Dir3.Y or Dir3.NegY, Dir3.Z or Dir3.NegZ) => Dir3.X,
            (Dir3.X or Dir3.NegX, Dir3.X or Dir3.NegX) => Dir3.Y,
            (Dir3.Y or Dir3.NegY, Dir3.Y or Dir3.NegY) => Dir3.X,
            _ => Dir3.Y,
        };


        public static int SelectAabb(this Dir3 dir, Aabb aabb) => dir switch
        {
            Dir3.X => aabb.Max.x,
            Dir3.NegX => aabb.Min.x,
            Dir3.Y => aabb.Max.y,
            Dir3.NegY => aabb.Min.y,
            Dir3.Z => aabb.Max.z,
            _ => aabb.Min.z,
        };

        public static int3 SelectAabbWith(this Dir3 dir, Aabb aabb, int3 other) => dir switch
        {
            Dir3.X => new int3(aabb.Max.x, other.y, other.z),
            Dir3.NegX => new int3(aabb.Min.x, other.y, other.z),
            Dir3.Y => new int3(other.x, aabb.Max.y, other.z),
            Dir3.NegY => new int3(other.x, aabb.Min.y, other.z),
            Dir3.Z => new int3(other.x, other.y, aabb.Max.z),
            _ => new int3(other.x, other.y, aabb.Min.z),
        };

        public static (Aabb first, Aabb second) SplitAabbOffset(this Dir3 dir, Aabb aabb, int offset)
        {
            return dir switch
            {
                Dir3.X =>
                    (new Aabb(aabb.Min, new int3(aabb.Min.x + offset, aabb.Max.y, aabb.Max.z)),
                     new Aabb(new int3(aabb.Min.x + offset, aabb.Min.y, aabb.Min.z), aabb.Max)),
                Dir3.Y =>
                    (new Aabb(aabb.Min, new int3(aabb.Max.x, aabb.Min.y + offset, aabb.Max.z)),
                     new Aabb(new int3(aabb.Min.x, aabb.Min.y + offset, aabb.Min.z), aabb.Max)),
                Dir3.Z =>
                    (new Aabb(aabb.Min, new int3(aabb.Max.x, aabb.Max.y, aabb.Min.z + offset)),
                     new Aabb(new int3(aabb.Min.x, aabb.Min.y, aabb.Min.z + offset), aabb.Max)),
                Dir3.NegX =>
                    (new Aabb(new int3(aabb.Max.x - offset, aabb.Min.y, aabb.Min.z), aabb.Max),
                     new Aabb(aabb.Min, new int3(aabb.Max.x - offset, aabb.Max.y, aabb.Max.z))),
                Dir3.NegY =>
                    (new Aabb(new int3(aabb.Min.x, aabb.Max.y - offset, aabb.Min.z), aabb.Max),
                     new Aabb(aabb.Min, new int3(aabb.Max.x, aabb.Max.y - offset, aabb.Max.z))),
                _ =>
                    (new Aabb(new int3(aabb.Min.x, aabb.Min.y, aabb.Max.z - offset), aabb.Max),
                     new Aabb(aabb.Min, new int3(aabb.Max.x, aabb.Max.y, aabb.Max.z - offset))),

            };
        }

        public static Aabb ExtendAabb(this Dir3 dir, Aabb aabb, int amount)
        {
            int3 offset = dir.ToVec3() * amount;
            return dir.IsPositive()
                ? new Aabb(aabb.Min, aabb.Max + offset)
                : new Aabb(aabb.Min + offset, aabb.Max);
        }

        public static Aabb TrimAabb(this Dir3 dir, Aabb aabb, int amount)
            => dir.Opposite().ExtendAabb(aabb, -amount);

            int3 off = dir.ToVec3() * amount;
            return dir.IsPositive()
                ? new Aabb(aabb.Min, aabb.Max + off)
                : new Aabb(aabb.Min + off, aabb.Max);
        }

    }
}
