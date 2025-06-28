using System;

namespace VelorenPort.CoreEngine.figure
{
    /// <summary>
    /// Basic material types used by figure voxels.
    /// This mirrors the Material enum in the original Rust project but
    /// only includes a minimal subset required by other modules.
    /// </summary>
    [Serializable]
    public enum Material
    {
        Skin,
        SkinDark,
        SkinLight,
        Hair,
        EyeDark,
        EyeLight,
        EyeWhite,
    }
}
