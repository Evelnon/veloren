using System;
using System.Collections.Generic;
using VelorenPort.CoreEngine.ECS;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Records entities that died during the previous tick along with their last
    /// known position. Used by gameplay systems similar to the Rust resource.
    /// </summary>
    [Serializable]
    public struct EntityDeathInfo {
        public Entity Entity;
        public Pos Position;
        public EntityDeathInfo(Entity entity, Pos position) {
            Entity = entity;
            Position = position;
        }
    }

    [Serializable]
    public class EntitiesDiedLastTick {
        public List<EntityDeathInfo> Entities { get; } = new();
    }
}
