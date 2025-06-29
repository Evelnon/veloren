using VelorenPort.NativeMath;
using Unity.Entities;

namespace VelorenPort.Server.Events;

/// <summary>
/// Simplified representation of the Rust CreateItemDropEvent.
/// Only includes the world position of the drop for now.
/// </summary>
public readonly record struct CreateItemDropEvent(float3 Position);
