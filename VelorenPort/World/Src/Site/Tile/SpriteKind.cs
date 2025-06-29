using System;

namespace VelorenPort.World.Site.Tile
{
    /// <summary>
    /// Minimal sprite kinds used for decoration placement. This only covers a
    /// few options from the full Rust enumeration.
    /// </summary>
    [Serializable]
    public enum SpriteKind
    {
        None,
        Tree,
        Lantern,
        Fence,
        Crop,
    }
}
