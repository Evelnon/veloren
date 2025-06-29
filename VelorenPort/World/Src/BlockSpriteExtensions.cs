using VelorenPort.CoreEngine.comp.terrain;

namespace VelorenPort.World {
    /// <summary>Placeholder extension to attach a sprite to a block.</summary>
    public static class BlockSpriteExtensions {
        public static Block WithSprite(this Block block, SpriteKind sprite)
        {
            var data = block.Data;
            return new Block(block.Kind, (byte)sprite, data[1], data[2]);
        }
    }
}
