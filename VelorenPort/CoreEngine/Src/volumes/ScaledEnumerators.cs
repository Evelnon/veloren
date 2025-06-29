using System.Collections.Generic;
using VelorenPort.NativeMath;

namespace VelorenPort.CoreEngine.Volumes;

/// <summary>
/// Enumerator for scaled positions.
/// </summary>
public struct ScaledPosEnumerator : IEnumerable<int3>
{
    private readonly int3 _min;
    private readonly int3 _max;
    private readonly int _scale;

    public ScaledPosEnumerator(int3 min, int3 max, int scale)
    {
        _min = min;
        _max = max;
        _scale = scale <= 0 ? 1 : scale;
    }

    public IEnumerator<int3> GetEnumerator()
    {
        for (int x = _min.x; x < _max.x; x += _scale)
        for (int y = _min.y; y < _max.y; y += _scale)
        for (int z = _min.z; z < _max.z; z += _scale)
            yield return new int3(x, y, z);
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
