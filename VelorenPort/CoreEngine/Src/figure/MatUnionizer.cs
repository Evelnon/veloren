using System.Collections.Generic;
using VelorenPort.NativeMath;

namespace VelorenPort.CoreEngine.figure;

/// <summary>
/// Unionizes multiple <see cref="Segment"/> volumes of <see cref="MatCell"/>.
/// This is a small subset of the Rust unionizer and only keeps filled cells.
/// </summary>
public static class MatUnionizer
{
    public static Segment Union(IEnumerable<(Segment seg, int3 offset)> inputs)
    {
        using var e = inputs.GetEnumerator();
        if (!e.MoveNext()) return new Segment(new int3(1,1,1));
        var first = e.Current;
        int3 min = first.offset;
        int3 max = first.offset + first.seg.Size;
        var list = new List<(Segment seg, int3 off)> { first };
        while (e.MoveNext())
        {
            list.Add(e.Current);
            min = math.min(min, e.Current.offset);
            max = math.max(max, e.Current.offset + e.Current.seg.Size);
        }
        var size = max - min;
        var result = new Segment(size);
        foreach (var (seg, off) in list)
        {
            foreach (var (p,v) in seg.Cells())
            {
                if (v.IsFilled)
                    result.Set(p + off - min, v);
            }
        }
        return result;
    }
}
