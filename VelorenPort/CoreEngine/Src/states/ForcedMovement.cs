using System;
using VelorenPort.NativeMath;

namespace VelorenPort.CoreEngine.States;

/// <summary>
/// Types of movement enforced by abilities. This mirrors the ForcedMovement
/// enum from the Rust project but only implements a subset of behaviour.
/// </summary>
[Serializable]
public abstract record ForcedMovement
{
    public sealed record Forward(float Strength) : ForcedMovement;
    public sealed record Reverse(float Strength) : ForcedMovement;
    public sealed record Sideways(float Strength) : ForcedMovement;
    public sealed record DirectedReverse(float Strength) : ForcedMovement;
    public sealed record AntiDirectedForward(float Strength) : ForcedMovement;
    public sealed record Leap(float Vertical, float ForwardAmount, float Progress, MovementDirection Direction) : ForcedMovement;
    public sealed record Hover(float MoveInput) : ForcedMovement;
}

/// <summary>
/// Helper extensions for ForcedMovement.
/// </summary>
public static class ForcedMovementExt
{
    /// <summary>
    /// Scale the strength of the forced movement.
    /// </summary>
    public static ForcedMovement Mul(this ForcedMovement fm, float scalar) => fm switch
    {
        ForcedMovement.Forward m => m with { Strength = m.Strength * scalar },
        ForcedMovement.Reverse m => m with { Strength = m.Strength * scalar },
        ForcedMovement.Sideways m => m with { Strength = m.Strength * scalar },
        ForcedMovement.DirectedReverse m => m with { Strength = m.Strength * scalar },
        ForcedMovement.AntiDirectedForward m => m with { Strength = m.Strength * scalar },
        ForcedMovement.Leap m => m with { Vertical = m.Vertical * scalar, ForwardAmount = m.ForwardAmount * scalar },
        ForcedMovement.Hover m => m,
        _ => fm
    };
}

/// <summary>
/// Direction used by <see cref="ForcedMovement.Leap"/> to orient the leap.
/// </summary>
[Serializable]
public enum MovementDirection
{
    Look,
    Move
}

public static class MovementDirectionExt
{
    /// <summary>
    /// Normalize and return the given direction vector depending on the mode.
    /// When <paramref name="dir"/> is zero it returns the zero vector.
    /// </summary>
    public static float2 Get2dDir(this MovementDirection md, float2 lookDir, float2 moveDir)
    {
        float2 v = md switch
        {
            MovementDirection.Look => lookDir,
            MovementDirection.Move => moveDir,
            _ => moveDir
        };
        return math.lengthsq(v) > 0f ? math.normalize(v) : float2.zero;
    }
}
