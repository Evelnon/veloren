using System;
using VelorenPort.NativeMath;
using VelorenPort.CoreEngine;
using VelorenPort.World;
using VelorenPort.Server;
using VelorenPort.CoreEngine.comp.terrain;

namespace VelorenPort.Simulation {
    // Simple numeric identifiers matching the slotmap keys in Rust

    public enum VolumeKind { Terrain, Entity }
    public readonly struct VolumePos<T> {
        public VolumeKind Kind { get; }
        public int3 Pos { get; }
        public T? Entity { get; }
        public VolumePos(int3 pos) { Kind = VolumeKind.Terrain; Pos = pos; Entity = default; }
        public VolumePos(int3 pos, T entity) { Kind = VolumeKind.Entity; Pos = pos; Entity = entity; }
    }

    /// <summary>
    /// Marker interface for simulation events. Each event specifies the system
    /// data that handlers require in order to process the event.
    /// </summary>
    public interface IEvent<TSystemData> { }

    /// <summary>
    /// Context passed to rule callbacks when an event is emitted.
    /// </summary>
    public sealed class EventCtx<R, E, D> where R : IRule where E : IEvent<D> {
        public RtState State { get; }
        public R Rule { get; }
        public E Event { get; }
        public World.World World { get; }
        public TestWorld.IndexRef Index { get; }
        public D SystemData { get; }

        public EventCtx(RtState state, R rule, E e, World.World world, TestWorld.IndexRef index, D data) {
            State = state;
            Rule = rule;
            Event = e;
            World = world;
            Index = index;
            SystemData = data;
        }
    }

    public sealed record OnSetup() : IEvent<Unit>;

    public sealed record OnTick(TimeOfDay TimeOfDay, Time Time, ulong Tick, float Dt) : IEvent<NpcSystemData>;

    public sealed record OnDeath(Actor Actor, float3? Wpos, Actor? Killer) : IEvent<Unit>;

    public sealed record OnHelped(Actor Actor, Actor? Saver) : IEvent<Unit>;

    public sealed record OnHealthChange(Actor Actor, Actor? Cause, float NewHealthFraction, float Change) : IEvent<Unit>;

    public sealed record OnTheft(Actor Actor, int3 Wpos, SpriteKind Sprite, SiteId? Site) : IEvent<Unit>;

    public sealed record OnMountVolume(Actor Actor, VolumePos<NpcId> Pos) : IEvent<Unit>;

}
