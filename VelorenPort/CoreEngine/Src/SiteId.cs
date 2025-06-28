using System;

namespace VelorenPort.CoreEngine
{
    /// <summary>Identifier used by NPCs to reference home sites.</summary>
    [Serializable]
    public readonly record struct SiteId(uint Value);
}
