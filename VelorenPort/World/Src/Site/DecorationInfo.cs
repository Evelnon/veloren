using System;
using VelorenPort.NativeMath;
using VelorenPort.World.Site.Tile;

namespace VelorenPort.World.Site
{
    /// <summary>Local decoration inside a site.</summary>
    [Serializable]
    public record DecorationInfo(int2 LocalPos, SpriteKind Sprite);
}
