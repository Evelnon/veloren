using System.Collections.Generic;

namespace VelorenPort.Server.Rtsim.Event;

/// <summary>
/// Event triggered when blocks change in the world. The payload here is highly
/// simplified compared to the Rust implementation.
/// </summary>
public record OnBlockChange(int Count);
