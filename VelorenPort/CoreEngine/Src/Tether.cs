using System;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Marker component for an entity acting as tether leader.
    /// </summary>
    [Serializable]
    public class Leader : IRole<Tethered> { }

    /// <summary>
    /// Marker component for an entity acting as tether follower.
    /// </summary>
    [Serializable]
    public class Follower : IRole<Tethered> { }

    /// <summary>
    /// Link representing a tether between two entities.
    /// Ported from <c>tether.rs</c>.
    /// </summary>
    [Serializable]
    public class Tethered : ILink<Tethered, TetherError, Tethered.CreateData, Tethered.PersistData, Tethered.DeleteData> {
        public Uid Leader { get; set; }
        public Uid Follower { get; set; }
        public float TetherLength { get; set; }

        public Tethered(Uid leader, Uid follower, float tetherLength) {
            Leader = leader;
            Follower = follower;
            TetherLength = tetherLength;
        }

        // Data passed when creating a tether
        public struct CreateData {
            public IdMaps IdMaps;
            public GenericWriteStorage<Is<Leader>> Leaders;
            public GenericWriteStorage<Is<Follower>> Followers;
            public ReadStorage<Is<Rider>> Riders;
            public ReadStorage<Is<VolumeRider>> VolumeRiders;
        }

        // Data used while persisting
        public struct PersistData {
            public IdMaps IdMaps;
            public Entities Entities;
            public ReadStorage<comp.Health> Healths;
            public ReadStorage<Is<Leader>> Leaders;
            public ReadStorage<Is<Follower>> Followers;
        }

        // Data used when deleting a tether
        public struct DeleteData {
            public IdMaps IdMaps;
            public GenericWriteStorage<Is<Leader>> Leaders;
            public GenericWriteStorage<Is<Follower>> Followers;
        }

        public TetherError Create(LinkHandle<Tethered, TetherError, CreateData, PersistData, DeleteData> handle, ref CreateData data) {
            var leaderEntity = data.IdMaps.GetEntity(Leader);
            var followerEntity = data.IdMaps.GetEntity(Follower);
            if (Leader.Equals(Follower)) {
                return TetherError.NotTetherable;
            }
            if (leaderEntity.HasValue && followerEntity.HasValue) {
                var riderPresent = data.Riders.Contains(followerEntity.Value) ||
                    data.VolumeRiders.Contains(followerEntity.Value);
                var followerHasFollower = data.Followers.Contains(followerEntity.Value);
                var followerIsLeader = data.Leaders.Contains(followerEntity.Value);
                var leaderIsFollower = data.Followers.Contains(leaderEntity.Value);
                if (!riderPresent && !followerHasFollower && (!followerIsLeader || !leaderIsFollower)) {
                    data.Leaders.Insert(leaderEntity.Value, handle.MakeRole<Leader>());
                    data.Followers.Insert(followerEntity.Value, handle.MakeRole<Follower>());
                    return TetherError.None;
                }
                return TetherError.NotTetherable;
            }
            return TetherError.NoSuchEntity;
        }

        public bool Persist(LinkHandle<Tethered, TetherError, CreateData, PersistData, DeleteData> handle, ref PersistData data) {
            var leaderEntity = data.IdMaps.GetEntity(Leader);
            var followerEntity = data.IdMaps.GetEntity(Follower);
            if (leaderEntity.HasValue && followerEntity.HasValue) {
                bool IsAlive(Unity.Entities.Entity e) => data.Entities.IsAlive(e) && (!data.Healths.TryGet(e, out var h) || !h.IsDead);
                return IsAlive(leaderEntity.Value)
                    && IsAlive(followerEntity.Value)
                    && data.Leaders.Contains(leaderEntity.Value)
                    && data.Followers.Contains(followerEntity.Value);
            }
            return false;
        }

        public void Delete(LinkHandle<Tethered, TetherError, CreateData, PersistData, DeleteData> handle, ref DeleteData data) {
            var leaderEntity = data.IdMaps.GetEntity(Leader);
            var followerEntity = data.IdMaps.GetEntity(Follower);
            if (leaderEntity.HasValue) data.Leaders.Remove(leaderEntity.Value);
            if (followerEntity.HasValue) data.Followers.Remove(followerEntity.Value);
        }
    }

    [Serializable]
    public enum TetherError {
        None,
        NoSuchEntity,
        NotTetherable,
    }
}
