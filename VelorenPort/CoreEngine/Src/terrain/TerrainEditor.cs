using VelorenPort.NativeMath;

namespace VelorenPort.CoreEngine.Terrain;

/// <summary>
/// Interface for simple voxel editors. Real terrain editing is much more
/// extensive in the Rust project; this provides a minimal subset.
/// </summary>
public interface ITerrainEditor<T>
{
    void Apply(IWriteVol<T> volume, int3 origin);
}
