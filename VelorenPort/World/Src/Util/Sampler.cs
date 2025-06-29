namespace VelorenPort.World.Util;

/// <summary>
/// Generic sampler interface mirroring Rust's <c>Sampler</c> trait.
/// </summary>
public interface ISampler<in TIndex, out TSample>
{
    TSample Get(TIndex index);
}

/// <summary>
/// Mutable sampler interface comparable to Rust's <c>SamplerMut</c>.
/// </summary>
public interface ISamplerMut<TIndex, TSample>
{
    TSample Get(TIndex index);
}
