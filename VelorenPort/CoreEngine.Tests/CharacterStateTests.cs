using VelorenPort.CoreEngine;
using VelorenPort.CoreEngine.States;

namespace CoreEngine.Tests;

public class CharacterStateTests
{
    [Fact]
    public void BasicMelee_IsAttackAndWielded()
    {
        CharacterState state = new CharacterState.BasicMelee(StageSection.Action);
        Assert.True(state.IsAttack());
        Assert.True(state.IsWielded());
        Assert.Equal(StageSection.Action, state.GetStageSection());
    }

    [Fact]
    public void Idle_NotAttackAndCanInteract()
    {
        CharacterState state = new CharacterState.Idle();
        Assert.False(state.IsAttack());
        Assert.True(state.CanInteract());
        Assert.Null(state.GetStageSection());
    }

    [Fact]
    public void SneakingIdle_IsStealthy()
    {
        CharacterState state = new CharacterState.Idle(true);
        Assert.True(state.IsStealthy());
    }

    [Fact]
    public void Glide_StageSectionReturned()
    {
        CharacterState state = new CharacterState.Glide(StageSection.Movement);
        Assert.Equal(StageSection.Movement, state.GetStageSection());
    }

    [Fact]
    public void SneakingWielding_IsStealthyAndWielded()
    {
        CharacterState state = new CharacterState.Wielding(true);
        Assert.True(state.IsStealthy());
        Assert.True(state.IsWielded());
        Assert.True(state.CanInteract());
    }

    [Fact]
    public void DashMelee_AttackAndWielded()
    {
        CharacterState state = new CharacterState.DashMelee(StageSection.Charge);
        Assert.True(state.IsAttack());
        Assert.True(state.IsWielded());
        Assert.Equal(StageSection.Charge, state.GetStageSection());
    }

    [Fact]
    public void Wallrun_WieldedFromData()
    {
        CharacterState state = new CharacterState.Wallrun(StageSection.Movement, true);
        Assert.True(state.IsWielded());
        Assert.Equal(StageSection.Movement, state.GetStageSection());
    }

    [Fact]
    public void UseItem_NotAttackButWielded()
    {
        CharacterState state = new CharacterState.UseItem(StageSection.Action, true);
        Assert.False(state.IsAttack());
        Assert.True(state.IsWielded());
        Assert.Equal(StageSection.Action, state.GetStageSection());
    }

    [Fact]
    public void ShouldFollowLook_ForBoostAndAttack()
    {
        CharacterState state = new CharacterState.Boost(StageSection.Movement);
        Assert.True(state.ShouldFollowLook());

        state = new CharacterState.BasicRanged(StageSection.Action);
        Assert.True(state.ShouldFollowLook());
    }

    [Fact]
    public void IsAimedAndUsingHands()
    {
        CharacterState state = new CharacterState.BasicRanged(StageSection.Action);
        Assert.True(state.IsAimed());

        state = new CharacterState.Talk();
        Assert.True(state.IsUsingHands());
    }

    [Fact]
    public void Roll_Movement_IsDodge()
    {
        CharacterState state = new CharacterState.Roll(StageSection.Movement, false, false);
        Assert.True(state.IsDodge());
    }

    [Fact]
    public void SpecificMovementStates()
    {
        CharacterState state = new CharacterState.Glide(StageSection.Action);
        Assert.True(state.IsGlide());

        state = new CharacterState.Skate(StageSection.Action);
        Assert.True(state.IsSkate());

        state = new CharacterState.Music(StageSection.Action);
        Assert.True(state.IsMusic());
    }

    [Fact]
    public void AttackFilters_AppliesCorrectly()
    {
        AttackFilters filters = new AttackFilters { Melee = true, Projectiles = false };
        Assert.True(filters.Applies(AttackSource.Melee));
        Assert.False(filters.Applies(AttackSource.Projectile));
    }
}
