using System;
using VelorenPort.CoreEngine;

namespace VelorenPort.World.Site.Economy;

/// <summary>
/// Associates a <see cref="Good"/> with a quantity.
/// </summary>
[Serializable]
public record TradeGood(Good Good, float Amount);
