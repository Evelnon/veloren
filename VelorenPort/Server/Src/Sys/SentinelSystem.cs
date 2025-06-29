using System.Collections.Generic;
using Unity.Entities;
using VelorenPort.CoreEngine;

namespace VelorenPort.Server.Sys;

/// <summary>
/// Very simple change detection system that records which entities gained,
/// lost or modified certain components since the last tick. This roughly
/// mirrors the behaviour of <c>server/src/sys/sentinel.rs</c>.
/// </summary>
public class SentinelSystem
{
    public class Trackers
    {
        public UpdateTracker<Uid> Uids { get; } = new();
        public UpdateTracker<Pos> Positions { get; } = new();
        public UpdateTracker<Vel> Velocities { get; } = new();
        public UpdateTracker<Ori> Orientations { get; } = new();
    }

    /// <summary>Update all trackers by diffing against the current entity state.</summary>
    public static void Update(EntityManager em, Trackers trackers)
    {
        trackers.Uids.Track(GetEntities<Uid>(em));
        trackers.Positions.Track(GetEntities<Pos>(em));
        trackers.Velocities.Track(GetEntities<Vel>(em));
        trackers.Orientations.Track(GetEntities<Ori>(em));
    }

    private static HashSet<Entity> GetEntities<T>(EntityManager em) where T : struct
    {
        var set = new HashSet<Entity>();
        foreach (var e in em.GetEntitiesWith<T>())
            set.Add(e);
        return set;
    }
}
