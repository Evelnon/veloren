using System;

namespace VelorenPort.CoreEngine
{
    /// <summary>
    /// Identifier for factions used by rtsim entities.
    /// </summary>
    [Serializable]
    public readonly record struct FactionId(uint Value);
}
