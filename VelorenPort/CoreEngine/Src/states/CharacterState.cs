using System;

namespace VelorenPort.CoreEngine.States
{

/// <summary>
/// Simplified representation of character states used during gameplay.
/// This only covers a small subset of the Rust implementation and is
/// intended as a starting point for the full port.
/// </summary>
[Serializable]
public abstract record CharacterState
{
    // Idle state, optionally sneaking.
    public sealed record Idle(bool IsSneaking = false) : CharacterState;
    public sealed record Crawl : CharacterState;
    public sealed record Climb(StageSection Stage, bool WasWielded) : CharacterState;
    public sealed record Sit : CharacterState;
    public sealed record Dance : CharacterState;
    public sealed record Talk : CharacterState;
    public sealed record Glide(StageSection Stage) : CharacterState;
    public sealed record GlideWield(StageSection Stage) : CharacterState;
    public sealed record Wielding(bool IsSneaking) : CharacterState;
    public sealed record BasicBlock(StageSection Stage) : CharacterState;
    public sealed record Equipping : CharacterState;
    public sealed record BasicMelee(StageSection Stage) : CharacterState;
    public sealed record BasicRanged(StageSection Stage) : CharacterState;
    public sealed record Throw(StageSection Stage) : CharacterState;
    public sealed record Roll(StageSection Stage, bool WasWielded, bool IsSneaking) : CharacterState;
    public sealed record Stunned(StageSection Stage, bool WasWielded) : CharacterState;
    public sealed record Shockwave(StageSection Stage) : CharacterState;
    public sealed record BasicBeam(StageSection Stage) : CharacterState;
    public sealed record BasicAura(StageSection Stage) : CharacterState;
    public sealed record StaticAura(StageSection Stage) : CharacterState;
    public sealed record Blink(StageSection Stage) : CharacterState;
    public sealed record BasicSummon(StageSection Stage) : CharacterState;
    public sealed record SelfBuff(StageSection Stage) : CharacterState;
    public sealed record Boost(StageSection Stage) : CharacterState;
    public sealed record DashMelee(StageSection Stage) : CharacterState;
    public sealed record ComboMelee2(StageSection Stage) : CharacterState;
    public sealed record LeapMelee(StageSection Stage) : CharacterState;
    public sealed record LeapShockwave(StageSection Stage) : CharacterState;
    public sealed record ChargedRanged(StageSection Stage) : CharacterState;
    public sealed record ChargedMelee(StageSection Stage) : CharacterState;
    public sealed record RepeaterRanged(StageSection Stage) : CharacterState;
    public sealed record SpriteSummon(StageSection Stage) : CharacterState;
    public sealed record UseItem(StageSection Stage, bool WasWielded) : CharacterState;
    public sealed record Interact(StageSection Stage, bool WasWielded) : CharacterState;
    public sealed record Wallrun(StageSection Stage, bool WasWielded) : CharacterState;
    public sealed record Skate(StageSection Stage) : CharacterState;
    public sealed record Music(StageSection Stage) : CharacterState;
    public sealed record FinisherMelee(StageSection Stage) : CharacterState;
    public sealed record DiveMelee(StageSection Stage) : CharacterState;
    public sealed record RiposteMelee(StageSection Stage) : CharacterState;
    public sealed record RapidMelee(StageSection Stage) : CharacterState;
    public sealed record Transform(StageSection Stage) : CharacterState;
    public sealed record RegrowHead(StageSection Stage) : CharacterState;
}

public static class CharacterStateExtensions
{
    /// <summary>Returns true if the state represents an attack animation.</summary>
    public static bool IsAttack(this CharacterState state) => state switch
    {
        CharacterState.BasicMelee => true,
        CharacterState.BasicRanged => true,
        CharacterState.DashMelee => true,
        CharacterState.ComboMelee2 => true,
        CharacterState.LeapMelee => true,
        CharacterState.LeapShockwave => true,
        CharacterState.ChargedMelee => true,
        CharacterState.ChargedRanged => true,
        CharacterState.RepeaterRanged => true,
        CharacterState.Throw => true,
        CharacterState.Shockwave => true,
        CharacterState.BasicBeam => true,
        CharacterState.BasicAura => true,
        CharacterState.StaticAura => true,
        CharacterState.Blink => true,
        CharacterState.BasicSummon => true,
        CharacterState.SpriteSummon => true,
        CharacterState.SelfBuff => true,
        CharacterState.FinisherMelee => true,
        CharacterState.DiveMelee => true,
        CharacterState.RiposteMelee => true,
        CharacterState.RapidMelee => true,
        _ => false
    };

    /// <summary>Returns true if the entity was wielding a weapon in this state.</summary>
    public static bool IsWielded(this CharacterState state) => state switch
    {
        CharacterState.Wielding => true,
        CharacterState.BasicMelee => true,
        CharacterState.BasicRanged => true,
        CharacterState.DashMelee => true,
        CharacterState.ComboMelee2 => true,
        CharacterState.LeapMelee => true,
        CharacterState.LeapShockwave => true,
        CharacterState.ChargedMelee => true,
        CharacterState.ChargedRanged => true,
        CharacterState.RepeaterRanged => true,
        CharacterState.Throw => true,
        CharacterState.BasicBlock => true,
        CharacterState.Shockwave => true,
        CharacterState.BasicBeam => true,
        CharacterState.BasicAura => true,
        CharacterState.StaticAura => true,
        CharacterState.Blink => true,
        CharacterState.BasicSummon => true,
        CharacterState.SpriteSummon => true,
        CharacterState.SelfBuff => true,
        CharacterState.FinisherMelee => true,
        CharacterState.DiveMelee => true,
        CharacterState.RiposteMelee => true,
        CharacterState.RapidMelee => true,
        CharacterState.Music => true,
        CharacterState.Roll { WasWielded: true } => true,
        CharacterState.Climb { WasWielded: true } => true,
        CharacterState.Stunned { WasWielded: true } => true,
        CharacterState.Wallrun { WasWielded: true } => true,
        CharacterState.UseItem { WasWielded: true } => true,
        CharacterState.Interact { WasWielded: true } => true,
        _ => false
    };

    /// <summary>Whether the entity can interact with objects while in this state.</summary>
    public static bool CanInteract(this CharacterState state) => state switch
    {
        CharacterState.Idle => true,
        CharacterState.Sit => true,
        CharacterState.Dance => true,
        CharacterState.Talk => true,
        CharacterState.Equipping => true,
        CharacterState.Wielding => true,
        CharacterState.GlideWield => true,
        _ => false
    };

    /// <summary>Whether the entity is currently stunned.</summary>
    public static bool IsStunned(this CharacterState state) => state is CharacterState.Stunned;

    /// <summary>Returns true if the state is considered stealthy.</summary>
    public static bool IsStealthy(this CharacterState state) => state switch
    {
        CharacterState.Idle { IsSneaking: true } => true,
        CharacterState.Wielding { IsSneaking: true } => true,
        CharacterState.Roll { IsSneaking: true } => true,
        _ => false
    };

    /// <summary>Stage section if the state supports it, otherwise null.</summary>
    public static StageSection? GetStageSection(this CharacterState state) => state switch
    {
        CharacterState.Climb s => s.Stage,
        CharacterState.Glide s => s.Stage,
        CharacterState.GlideWield s => s.Stage,
        CharacterState.BasicBlock s => s.Stage,
        CharacterState.BasicMelee s => s.Stage,
        CharacterState.BasicRanged s => s.Stage,
        CharacterState.DashMelee s => s.Stage,
        CharacterState.ComboMelee2 s => s.Stage,
        CharacterState.LeapMelee s => s.Stage,
        CharacterState.LeapShockwave s => s.Stage,
        CharacterState.ChargedMelee s => s.Stage,
        CharacterState.ChargedRanged s => s.Stage,
        CharacterState.RepeaterRanged s => s.Stage,
        CharacterState.Throw s => s.Stage,
        CharacterState.Roll s => s.Stage,
        CharacterState.Stunned s => s.Stage,
        CharacterState.Boost s => s.Stage,
        CharacterState.Shockwave s => s.Stage,
        CharacterState.BasicBeam s => s.Stage,
        CharacterState.BasicAura s => s.Stage,
        CharacterState.StaticAura s => s.Stage,
        CharacterState.Blink s => s.Stage,
        CharacterState.SpriteSummon s => s.Stage,
        CharacterState.BasicSummon s => s.Stage,
        CharacterState.UseItem s => s.Stage,
        CharacterState.Interact s => s.Stage,
        CharacterState.Wallrun s => s.Stage,
        CharacterState.Skate s => s.Stage,
        CharacterState.Music s => s.Stage,
        CharacterState.FinisherMelee s => s.Stage,
        CharacterState.DiveMelee s => s.Stage,
        CharacterState.RiposteMelee s => s.Stage,
        CharacterState.RapidMelee s => s.Stage,
        CharacterState.Transform s => s.Stage,
        CharacterState.RegrowHead s => s.Stage,
        CharacterState.SelfBuff s => s.Stage,
        _ => null
    };


    /// <summary>True if the state is Boost or any attack, meaning the entity should follow look direction.</summary>
    public static bool ShouldFollowLook(this CharacterState state)
        => state is CharacterState.Boost || state.IsAttack();

    /// <summary>Returns true if the state requires aiming with the cursor.</summary>
    public static bool IsAimed(this CharacterState state) => state switch
    {
        CharacterState.BasicMelee => true,
        CharacterState.BasicRanged => true,
        CharacterState.DashMelee => true,
        CharacterState.ComboMelee2 => true,
        CharacterState.BasicBlock => true,
        CharacterState.LeapMelee => true,
        CharacterState.LeapShockwave => true,
        CharacterState.ChargedMelee => true,
        CharacterState.ChargedRanged => true,
        CharacterState.RepeaterRanged => true,
        CharacterState.Throw => true,
        CharacterState.Shockwave => true,
        CharacterState.BasicBeam => true,
        CharacterState.Stunned => true,
        CharacterState.Wielding => true,
        CharacterState.Talk => true,
        CharacterState.FinisherMelee => true,
        CharacterState.DiveMelee => true,
        CharacterState.RiposteMelee => true,
        CharacterState.RapidMelee => true,
        _ => false
    };

    /// <summary>True if the state involves using the hands directly.</summary>
    public static bool IsUsingHands(this CharacterState state) => state switch
    {
        CharacterState.Climb => true,
        CharacterState.Equipping => true,
        CharacterState.Dance => true,
        CharacterState.Glide => true,
        CharacterState.GlideWield => true,
        CharacterState.Talk => true,
        CharacterState.Roll => true,
        _ => false
    };

    /// <summary>Return whether this state counts as a dodge.</summary>
    public static bool IsDodge(this CharacterState state)
    {
        return state is CharacterState.Roll { Stage: StageSection.Buildup or StageSection.Movement };
    }

    /// <summary>Convenience checks for specific movement states.</summary>
    public static bool IsGlide(this CharacterState state) => state is CharacterState.Glide;
    public static bool IsSkate(this CharacterState state) => state is CharacterState.Skate;
    public static bool IsMusic(this CharacterState state) => state is CharacterState.Music;

    /// <summary>Whether this is a melee or beam attack.</summary>
    public static bool IsMeleeAttack(this CharacterState state) => state switch
    {
        CharacterState.BasicMelee => true,
        CharacterState.ComboMelee2 => true,
        CharacterState.LeapMelee => true,
        CharacterState.LeapShockwave => true,
        CharacterState.DashMelee => true,
        CharacterState.ChargedMelee => true,
        CharacterState.FinisherMelee => true,
        CharacterState.DiveMelee => true,
        CharacterState.RiposteMelee => true,
        CharacterState.RapidMelee => true,
        _ => false
    };

    public static bool IsBeamAttack(this CharacterState state) => state is CharacterState.BasicBeam;
}
}
