using System;
using Unity.Mathematics;

namespace VelorenPort.CoreEngine
{
    /// <summary>
    /// Current input state from the player controller.
    /// </summary>
    [Serializable]
    public struct ControllerInputs
    {
        public float2 MoveDir;
        public float MoveZ;
        public Dir LookDir;
        public float3? BreakBlockPos;
        public bool Strafing;

        /// <summary>Clamp move vector and vertical input to valid ranges.</summary>
        public void Sanitize()
        {
            if (math.any(!math.isfinite(MoveDir)))
                MoveDir = float2.zero;
            if (!math.isfinite(MoveZ))
                MoveZ = 0f;
            MoveDir = math.normalize(MoveDir) * math.min(1f, math.length(MoveDir));
            MoveZ = math.clamp(MoveZ, -1f, 1f);
        }

        public void UpdateWith(ControllerInputs other)
        {
            MoveDir = other.MoveDir;
            MoveZ = other.MoveZ;
            LookDir = other.LookDir;
            BreakBlockPos = other.BreakBlockPos;
        }
    }
}
