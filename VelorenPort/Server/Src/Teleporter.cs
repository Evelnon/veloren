namespace VelorenPort.Server
{
    /// <summary>
    /// Simple teleporter definition with a position and target destination.
    /// Mirrors the behavior of portal objects from the Rust server but
    /// without aggro checks or buildup time.
    /// </summary>
    public struct Teleporter
    {
        public VelorenPort.NativeMath.float3 Position;
        public VelorenPort.NativeMath.float3 Target;
    }
}
