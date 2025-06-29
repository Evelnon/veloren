using System;

namespace VelorenPort.CoreEngine.comp
{
    /// <summary>
    /// Basic object component used by simple server systems. Mirrors a subset of
    /// <c>common/src/comp/misc.rs</c> from the Rust project.
    /// </summary>
    [Serializable]
    public struct Object
    {
        public ObjectKind Kind;
        public DateTime SpawnedAt;
        public TimeSpan Timeout;
        public Unity.Mathematics.float3 Target;
        public bool RequiresNoAggro;
        public float BuildupTime;

        public static Object DeleteAfter(TimeSpan timeout)
        {
            return new Object
            {
                Kind = ObjectKind.DeleteAfter,
                SpawnedAt = DateTime.UtcNow,
                Timeout = timeout
            };
        }

        public static Object Portal(Unity.Mathematics.float3 target,
                                    bool requiresNoAggro,
                                    float buildupSeconds)
        {
            return new Object
            {
                Kind = ObjectKind.Portal,
                Target = target,
                RequiresNoAggro = requiresNoAggro,
                BuildupTime = buildupSeconds
            };
        }
    }

    public enum ObjectKind
    {
        DeleteAfter,
        Portal,
    }
}
