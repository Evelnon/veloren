using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Mirrors <c>HealthChangeInfo</c> from Rust.
    /// </summary>
    [Serializable]
    public struct HealthChangeInfo {
        public float Amount;
        public bool Precise;
        public Uid Target;
        public combat.DamageContributor? By;
        public DamageSource? Cause;
        public ulong Instance;
    }

    /// <summary>
    /// Enumeration of discrete outcomes from in-game events. Ported from
    /// <c>outcome.rs</c>.
    /// </summary>
    [Serializable]
    public abstract record Outcome {
        [Serializable]
        public sealed record Explosion(float3 Pos, float Power, float Radius, bool IsAttack, comp.item.Reagent? Reagent) : Outcome;
        [Serializable]
        public sealed record Lightning(float3 Pos) : Outcome;
        [Serializable]
        public sealed record ProjectileShot(float3 Pos, comp.Body Body, float3 Vel) : Outcome;
        [Serializable]
        public sealed record ProjectileHit(float3 Pos, comp.Body Body, float3 Vel, Uid? Source, Uid? Target) : Outcome;
        [Serializable]
        public sealed record Beam(float3 Pos, comp.beam.FrontendSpecifier Specifier) : Outcome;
        [Serializable]
        public sealed record ExpChange(Uid Uid, uint Exp, HashSet<comp.skillset.SkillGroupKind> XpPools) : Outcome;
        [Serializable]
        public sealed record SkillPointGain(Uid Uid, comp.skillset.SkillGroupKind SkillTree, ushort TotalPoints) : Outcome;
        [Serializable]
        public sealed record ComboChange(Uid Uid, uint Combo) : Outcome;
        [Serializable]
        public sealed record BreakBlock(int3 Pos, comp.item.ToolKind? Tool, Rgb<byte>? Color) : Outcome;
        [Serializable]
        public sealed record DamagedBlock(int3 Pos, comp.item.ToolKind? Tool, bool StageChanged) : Outcome;
        [Serializable]
        public sealed record SummonedCreature(float3 Pos, comp.Body Body) : Outcome;
        [Serializable]
        public sealed record HealthChange(float3 Pos, HealthChangeInfo Info) : Outcome;
        [Serializable]
        public sealed record Death(float3 Pos) : Outcome;
        [Serializable]
        public sealed record Block(float3 Pos, bool Parry, Uid Uid) : Outcome;
        [Serializable]
        public sealed record PoiseChange(float3 Pos, comp.poise.PoiseState State) : Outcome;
        [Serializable]
        public sealed record GroundSlam(float3 Pos) : Outcome;
        [Serializable]
        public sealed record IceSpikes(float3 Pos) : Outcome;
        [Serializable]
        public sealed record IceCrack(float3 Pos) : Outcome;
        [Serializable]
        public sealed record FlashFreeze(float3 Pos) : Outcome;
        [Serializable]
        public sealed record Steam(float3 Pos) : Outcome;
        [Serializable]
        public sealed record LaserBeam(float3 Pos) : Outcome;
        [Serializable]
        public sealed record CyclopsCharge(float3 Pos) : Outcome;
        [Serializable]
        public sealed record FlamethrowerCharge(float3 Pos) : Outcome;
        [Serializable]
        public sealed record FuseCharge(float3 Pos) : Outcome;
        [Serializable]
        public sealed record TerracottaStatueCharge(float3 Pos) : Outcome;
        [Serializable]
        public sealed record SurpriseEgg(float3 Pos) : Outcome;
        [Serializable]
        public sealed record Utterance(float3 Pos, comp.Body Body, comp.UtteranceKind Kind) : Outcome;
        [Serializable]
        public sealed record Glider(float3 Pos, bool Wielded) : Outcome;
        [Serializable]
        public sealed record SpriteDelete(int3 Pos, terrain.SpriteKind Sprite) : Outcome;
        [Serializable]
        public sealed record SpriteUnlocked(int3 Pos) : Outcome;
        [Serializable]
        public sealed record FailedSpriteUnlock(int3 Pos) : Outcome;
        [Serializable]
        public sealed record Whoosh(float3 Pos) : Outcome;
        [Serializable]
        public sealed record Swoosh(float3 Pos) : Outcome;
        [Serializable]
        public sealed record Slash(float3 Pos) : Outcome;
        [Serializable]
        public sealed record FireShockwave(float3 Pos) : Outcome;
        [Serializable]
        public sealed record GroundDig(float3 Pos) : Outcome;
        [Serializable]
        public sealed record PortalActivated(float3 Pos) : Outcome;
        [Serializable]
        public sealed record TeleportedByPortal(float3 Pos) : Outcome;
        [Serializable]
        public sealed record FromTheAshes(float3 Pos) : Outcome;
        [Serializable]
        public sealed record ClayGolemDash(float3 Pos) : Outcome;
        [Serializable]
        public sealed record Bleep(float3 Pos) : Outcome;
        [Serializable]
        public sealed record Charge(float3 Pos) : Outcome;
        [Serializable]
        public sealed record HeadLost(Uid Uid, ulong Head) : Outcome;
        [Serializable]
        public sealed record Splash(float3 Vel, float3 Pos, float Mass, comp.fluid_dynamics.LiquidKind Kind) : Outcome;
        [Serializable]
        public sealed record Transformation(float3 Pos) : Outcome;

        public float3? GetPos() => this switch {
            Explosion e => e.Pos,
            ProjectileShot p => p.Pos,
            ProjectileHit p => p.Pos,
            Beam b => b.Pos,
            SummonedCreature s => s.Pos,
            HealthChange h => h.Pos,
            Death d => d.Pos,
            Block b => b.Pos,
            PoiseChange p => p.Pos,
            GroundSlam g => g.Pos,
            FlashFreeze f => f.Pos,
            Whoosh w => w.Pos,
            Swoosh s => s.Pos,
            Slash s => s.Pos,
            Bleep b => b.Pos,
            Charge c => c.Pos,
            IceSpikes i => i.Pos,
            Steam s => s.Pos,
            FireShockwave f => f.Pos,
            IceCrack c => c.Pos,
            Utterance u => u.Pos,
            CyclopsCharge c => c.Pos,
            FlamethrowerCharge f => f.Pos,
            FuseCharge f => f.Pos,
            TerracottaStatueCharge t => t.Pos,
            SurpriseEgg s => s.Pos,
            LaserBeam l => l.Pos,
            GroundDig g => g.Pos,
            PortalActivated p => p.Pos,
            TeleportedByPortal p => p.Pos,
            FromTheAshes f => f.Pos,
            ClayGolemDash c => c.Pos,
            Glider g => g.Pos,
            Splash s => s.Pos,
            Transformation t => t.Pos,
            BreakBlock b => (float3)b.Pos + 0.5f,
            DamagedBlock d => (float3)d.Pos + 0.5f,
            SpriteUnlocked s => (float3)s.Pos + 0.5f,
            SpriteDelete s => (float3)s.Pos + 0.5f,
            FailedSpriteUnlock s => (float3)s.Pos + 0.5f,
            _ => null,
        };
    }
}
