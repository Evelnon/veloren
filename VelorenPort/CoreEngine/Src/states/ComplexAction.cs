using System;

namespace VelorenPort.CoreEngine.States;

/// <summary>
/// Placeholder for more advanced character actions. The Rust project
/// exposes many complex behaviours which are gradually ported here.
/// </summary>
[Serializable]
public abstract record ComplexAction(StageSection Stage)
{
    public sealed record Kick(StageSection Stage, float Strength) : ComplexAction(Stage);
    public sealed record Spin(StageSection Stage, float Duration) : ComplexAction(Stage);
    public sealed record JumpShot(StageSection Stage, float Power) : ComplexAction(Stage);
}

public static class ComplexActionExtensions
{
    public static bool IsOffensive(this ComplexAction action) => action switch
    {
        ComplexAction.Kick => true,
        ComplexAction.Spin => true,
        ComplexAction.JumpShot => true,
        _ => false
    };
}
