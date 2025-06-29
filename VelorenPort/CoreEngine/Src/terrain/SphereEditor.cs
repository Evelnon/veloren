using System;
using VelorenPort.NativeMath;

namespace VelorenPort.CoreEngine.Terrain;

/// <summary>
/// Basic spherical editor that fills a radius with the provided value.
/// </summary>
[Serializable]
public class SphereEditor<T> : ITerrainEditor<T>
{
    private readonly int _radius;
    private readonly T _value;

    public SphereEditor(int radius, T value)
    {
        _radius = radius;
        _value = value;
    }

    public void Apply(IWriteVol<T> volume, int3 origin)
    {
        int r2 = _radius * _radius;
        for (int x = -_radius; x <= _radius; x++)
        for (int y = -_radius; y <= _radius; y++)
        for (int z = -_radius; z <= _radius; z++)
        {
            if (x * x + y * y + z * z <= r2)
            {
                var pos = origin + new int3(x, y, z);
                if (volume.InBounds(pos))
                    volume.Set(pos, _value);
            }
        }
    }
}
