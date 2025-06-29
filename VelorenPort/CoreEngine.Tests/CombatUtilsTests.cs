using System.Collections.Generic;
using VelorenPort.CoreEngine;
using Xunit;

namespace CoreEngine.Tests;

public class CombatUtilsTests
{
    private class DummyTarget : IDamageable
    {
        public Uid Id { get; }
        public float Health { get; set; }
        public DummyTarget(Uid id, float health) { Id = id; Health = health; }
    }

    [Fact]
    public void ApplyAttack_AddsMissingTarget()
    {
        var health = new Dictionary<Uid, float>();
        var attack = new Attack(new Uid(1), new HitInfo(new Uid(2), 5f, DamageKind.Physical));
        CombatUtils.ApplyAttack(health, attack);
        Assert.Equal(-5f, health[new Uid(2)]);
    }

    [Fact]
    public void ApplyAttack_RespectsResistances()
    {
        var health = new Dictionary<Uid, float> { [new Uid(3)] = 0f };
        var attack = new Attack(new Uid(1), new HitInfo(new Uid(3), 10f, DamageKind.Fire));
        var resist = new Resistances(0f);
        resist[DamageKind.Fire] = 0.5f;
        CombatUtils.ApplyAttack(health, attack, resist);
        Assert.Equal(-5f, health[new Uid(3)]);
    }

    [Fact]
    public void Apply_ToDamageable_LogsEvent()
    {
        var target = new DummyTarget(new Uid(4), 20f);
        var attack = new Attack(new Uid(5), new HitInfo(target.Id, 10f, DamageKind.Physical));
        var resist = new Resistances(0f);
        resist[DamageKind.Physical] = 0.1f;
        var log = new List<DamageEvent>();
        CombatUtils.Apply(target, attack, resist, log);
        Assert.Equal(11f, target.Health);
        Assert.Single(log);
        Assert.Equal(target.Id, log[0].Target);
        Assert.InRange(log[0].Amount, 8.9f, 9.1f);
    }
}
