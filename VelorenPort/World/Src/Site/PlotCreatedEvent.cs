using System;
using VelorenPort.NativeMath;
using VelorenPort.CoreEngine;

namespace VelorenPort.World.Site
{
    /// <summary>Record of a plot created during world generation.</summary>
    [Serializable]
    public record PlotCreatedEvent(Store<Site>.Id SiteId, PlotKind Kind, int2 LocalPos);
}
