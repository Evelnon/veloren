using VelorenPort.NativeMath;
using Unity.Entities;

namespace VelorenPort.Server.Events;

/// <summary>
/// Simplified representation of the Rust CreateItemDropEvent.
/// Carries the item name and amount so the server can spawn the drop.
/// </summary>
public readonly record struct CreateItemDropEvent(float3 Position, string Item, uint Amount);
