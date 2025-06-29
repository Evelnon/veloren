using System;
using VelorenPort.NativeMath;
using VelorenPort.CoreEngine;
using VelorenPort.World.Site.Tile;

namespace VelorenPort.World.Site
{
    /// <summary>Record of a decoration placed during site generation.</summary>
    [Serializable]
    public record DecorationPlacedEvent(Store<Site>.Id SiteId, int2 LocalPos, SpriteKind Sprite);
}
