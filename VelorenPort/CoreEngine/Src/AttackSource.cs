using System;

namespace VelorenPort.CoreEngine;

/// <summary>
/// Source of an attack used for determining blocking and parry logic.
/// Mirrors the Rust <c>AttackSource</c> enum.
/// </summary>
[Serializable]
public enum AttackSource
{
    Melee,
    Projectile,
    Beam,
    GroundShockwave,
    AirShockwave,
    UndodgeableShockwave,
    Explosion,
}
