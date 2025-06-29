using System.Collections.Generic;
using VelorenPort.NativeMath;

namespace VelorenPort.CoreEngine.Volumes;

/// <summary>
/// Read-only view of another volume scaled by an integer factor.
/// </summary>
public class ScaledVol<T> : IReadVol<T>
{
    private readonly IReadVol<T> _inner;
    private readonly int _scale;

    public ScaledVol(IReadVol<T> inner, int scale)
    {
        _inner = inner;
        _scale = scale <= 0 ? 1 : scale;
    }

    private int3 ScaleDown(int3 v) => new int3(v.x / _scale, v.y / _scale, v.z / _scale);

    public bool InBounds(int3 pos) => _inner.InBounds(ScaleDown(pos));

    public T Get(int3 pos) => _inner.Get(ScaleDown(pos));

    public IEnumerable<(int3 Pos, T Vox)> Cells(int3 min, int3 max)
    {
        for (int x = min.x; x < max.x; x++)
        for (int y = min.y; y < max.y; y++)
        for (int z = min.z; z < max.z; z++)
        {
            var p = new int3(x, y, z);
            yield return (p, Get(p));
        }
    }
}
