using System;
using System.Collections.Generic;
using VelorenPort.NativeMath;

namespace VelorenPort.World.Site.Util {
    /// <summary>
    /// Helper struct for placing tileable sprite blocks on a 2D plane.
    /// This is a partial port of <c>world/src/site/util/sprites.rs</c>
    /// used by some plot templates. Many sprite features are not yet
    /// implemented but the structure mirrors the original API so the
    /// ported templates can compile.
    /// </summary>
    [Serializable]
    public class Tileable2 {
        private int alt;
        private Aabr bounds;
        private World.Block center;
        private readonly Dictionary<Dir, World.Block> side;
        private readonly Dictionary<Dir, World.Block> corner;
        private Dir rotation;

        public Tileable2() {
            alt = 0;
            bounds = new Aabr(int2.zero, int2.zero);
            center = World.Block.Air;
            side = new Dictionary<Dir, World.Block>{
                [Dir.X] = World.Block.Air,
                [Dir.NegX] = World.Block.Air,
                [Dir.Y] = World.Block.Air,
                [Dir.NegY] = World.Block.Air
            };
            corner = new Dictionary<Dir, World.Block>{
                [Dir.X] = World.Block.Air,
                [Dir.NegX] = World.Block.Air,
                [Dir.Y] = World.Block.Air,
                [Dir.NegY] = World.Block.Air
            };
            rotation = Dir.X;
        }

        public static Tileable2 Empty() => new Tileable2();

        public static Tileable2 TwoBy(int len, int3 pos, Dir dir) {
            return Empty().WithRotation(dir)
                .WithCenterSize(pos, new int2(len, 2));
        }

        public Tileable2 WithCenterSize(int3 centerPos, int2 size) {
            int2 extentMin = (size - 1) / 2;
            int2 extentMax = size / 2;
            var rot = rotation;
            bounds = new Aabr(
                centerPos.xy - rot.Vec2(extentMin.x, extentMin.y),
                centerPos.xy + rot.Vec2(extentMax.x, extentMax.y));
            alt = centerPos.z;
            return this;
        }

        public Tileable2 WithBounds(Aabr b) { bounds = b; return this; }
        public Tileable2 WithAlt(int a) { alt = a; return this; }
        public Tileable2 WithCenter(World.Block block) { center = block; return this; }
        public Tileable2 WithCenterSprite(World.SpriteKind sprite) => WithCenter(center.WithSprite(sprite));
        public Tileable2 WithSide(Dir dir, World.Block block) { side[dir] = block; return this; }
        public Tileable2 WithSideSprite(World.SpriteKind sprite)
        {
            foreach (var key in new[] { Dir.X, Dir.NegX, Dir.Y, Dir.NegY })
                side[key] = side[key].WithSprite(sprite);
            return this;
        }
        public Tileable2 WithSideDir(Dir dir, World.SpriteKind sprite)
        {
            side[dir] = side[dir].WithSprite(sprite);
            return this;
        }
        public Tileable2 WithSideAxis(Dir axis, World.SpriteKind sprite)
        {
            WithSideDir(axis, sprite);
            WithSideDir(axis.Opposite(), sprite);
            return this;
        }
        public Tileable2 WithCorner(Dir dir, World.Block block) { corner[dir] = block; return this; }
        public Tileable2 WithCornerSprite(World.SpriteKind sprite)
        {
            foreach (var key in new[] { Dir.X, Dir.NegX, Dir.Y, Dir.NegY })
                corner[key] = corner[key].WithSprite(sprite);
            return this;
        }
        public Tileable2 WithCornerDir(Dir dir, World.Block block) { corner[dir] = block; return this; }
        public Tileable2 WithCornerSpriteDir(Dir dir, World.SpriteKind sprite)
        {
            corner[dir] = corner[dir].WithSprite(sprite);
            return this;
        }
        public Tileable2 WithCornerSide(Dir axis, World.Block block)
        {
            WithCorner(axis, block);
            WithCorner(axis.RotatedCcw(), block);
            return this;
        }
        public Tileable2 WithCornerSpriteSide(Dir axis, World.SpriteKind sprite)
        {
            WithCornerSpriteDir(axis, sprite);
            WithCornerSpriteDir(axis.RotatedCcw(), sprite);
            return this;
        }
        public Tileable2 WithRotation(Dir dir) { rotation = dir; return this; }

        public int Alt => alt;
        public Aabr Bounds => bounds;
        public int2 Size => bounds.Max - bounds.Min + 1;
        public Dir Rotation => rotation;
        public World.Block Center => center;
        public World.Block Side(Dir dir) => side[dir.RelativeTo(rotation)];
        public World.Block Corner(Dir dir) => corner[dir.RelativeTo(rotation)];
    }
}
