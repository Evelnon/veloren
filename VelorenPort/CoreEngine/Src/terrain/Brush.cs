using VelorenPort.NativeMath;

namespace VelorenPort.CoreEngine.Terrain;

/// <summary>
/// Utility helpers for editing terrain volumes.
/// </summary>
public static class Brush
{
    /// <summary>
    /// Fill a sphere in <paramref name="volume"/> with <paramref name="value"/>.
    /// </summary>
    public static void FillSphere<T>(IWriteVol<T> volume, int3 origin, int radius, T value)
    {
        var editor = new SphereEditor<T>(radius, value);
        editor.Apply(volume, origin);
    }

    /// <summary>
    /// Fill an axis-aligned box in <paramref name="volume"/>.
    /// </summary>
    public static void FillBox<T>(IWriteVol<T> volume, int3 origin, int3 size, T value)
    {
        var editor = new BoxEditor<T>(size, value);
        editor.Apply(volume, origin);
    }
}
