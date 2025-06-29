using System;
using VelorenPort.NativeMath;

namespace VelorenPort.CoreEngine
{
    /// <summary>
    /// Additional data for queued inputs, such as selected position or target entity.
    /// </summary>
    [Serializable]
    public struct InputAttr
    {
        public float3? SelectPos;
        public Uid? TargetEntity;

        public InputAttr(float3? selectPos, Uid? targetEntity)
        {
            SelectPos = selectPos;
            TargetEntity = targetEntity;
        }
    }
}
