using System;

namespace VelorenPort.CoreEngine
{
    /// <summary>
    /// Represents the portion of an ability's animation. Used by many
    /// character states in the original Rust code.
    /// </summary>
    [Serializable]
    public enum StageSection
    {
        Buildup,
        Recover,
        Charge,
        Movement,
        Action,
    }
}
