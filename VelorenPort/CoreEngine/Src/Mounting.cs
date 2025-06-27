using System;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Represent the state of an entity mounting or dismounting another entity.
    /// </summary>
    [Serializable]
    public enum MountState {
        None,
        Mounting,
        Mounted,
        Dismounting
    }

    /// <summary>Information about a mount relationship.</summary>
    [Serializable]
    public struct MountInfo {
        public Uid Rider;
        public Uid Mount;
        public MountState State;

        public MountInfo(Uid rider, Uid mount, MountState state) {
            Rider = rider;
            Mount = mount;
            State = state;
        }

        public bool IsMounted => State == MountState.Mounted;

        public void UpdateState(MountState state) => State = state;
    }

    /// <summary>Utility for managing mount interactions.</summary>
    public static class MountingSystem {
        /// Attempt to mount <paramref name="rider"/> onto <paramref name="mount"/>.
        public static void Mount(ref MountInfo info) {
            if (info.State == MountState.None || info.State == MountState.Dismounting) {
                info.State = MountState.Mounting;
            }
        }

        /// Complete the mount action once the animation or delay is finished.
        public static void FinishMount(ref MountInfo info) {
            if (info.State == MountState.Mounting)
                info.State = MountState.Mounted;
        }

        /// Begin dismounting process.
        public static void Dismount(ref MountInfo info) {
            if (info.State == MountState.Mounted)
                info.State = MountState.Dismounting;
        }

        /// Finalise dismount and reset state.
        public static void FinishDismount(ref MountInfo info) {
            if (info.State == MountState.Dismounting)
                info.State = MountState.None;
        }
    }
}
