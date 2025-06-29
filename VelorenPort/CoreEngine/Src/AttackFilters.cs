using System;

namespace VelorenPort.CoreEngine;

/// <summary>
/// Flags specifying which kinds of attacks are affected. Used for roll
/// immunities and blocking logic.
/// </summary>
[Serializable]
public struct AttackFilters
{
    public bool Melee;
    public bool Projectiles;
    public bool Beams;
    public bool GroundShockwaves;
    public bool AirShockwaves;
    public bool Explosions;

    /// <summary>Return true if the filter includes the given attack source.</summary>
    public bool Applies(AttackSource attack) => attack switch
    {
        AttackSource.Melee => Melee,
        AttackSource.Projectile => Projectiles,
        AttackSource.Beam => Beams,
        AttackSource.GroundShockwave => GroundShockwaves,
        AttackSource.AirShockwave => AirShockwaves,
        AttackSource.UndodgeableShockwave => false,
        AttackSource.Explosion => Explosions,
        _ => false
    };
}
