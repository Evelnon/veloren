using System;
using VelorenPort.NativeMath;

namespace VelorenPort.CoreEngine.Terrain;

/// <summary>
/// Axis-aligned box editor that fills a region with the provided value.
/// </summary>
[Serializable]
public class BoxEditor<T> : ITerrainEditor<T>
{
    private readonly int3 _size;
    private readonly T _value;

    public BoxEditor(int3 size, T value)
    {
        _size = size;
        _value = value;
    }

    public void Apply(IWriteVol<T> volume, int3 origin)
    {
        for (int x = 0; x < _size.x; x++)
        for (int y = 0; y < _size.y; y++)
        for (int z = 0; z < _size.z; z++)
        {
            var pos = origin + new int3(x, y, z);
            if (volume.InBounds(pos))
                volume.Set(pos, _value);
        }
    }
}
