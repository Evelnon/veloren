using System;

namespace VelorenPort.CoreEngine
{
    /// <summary>
    /// Identifier used for NPC entities. Mirrors the NpcId slotmap key in Rust.
    /// </summary>
    [Serializable]
    public readonly record struct NpcId(int Value);
}
