using System;
using VelorenPort.NativeMath;

namespace VelorenPort.World
{
    /// <summary>
    /// Runtime representation of a fauna entity with a simple
    /// behaviour state machine.
    /// </summary>
    [Serializable]
    public class WildlifeEntity
    {
        public FaunaKind Kind { get; }
        public int3 Position { get; private set; }
        public FaunaBehaviourState State { get; private set; }
        private float _age;
        private readonly Random _rng = new();

        public WildlifeEntity(int3 position, FaunaKind kind)
        {
            Position = position;
            Kind = kind;
            State = FaunaBehaviourState.Spawning;
            _age = 0f;
        }

        /// <summary>Advance the behaviour state machine.</summary>
        public void Tick(float dt)
        {
            _age += dt;
            switch (State)
            {
                case FaunaBehaviourState.Spawning:
                    if (_age > 1f)
                        State = FaunaBehaviourState.Roaming;
                    break;
                case FaunaBehaviourState.Roaming:
                    if (_age > 60f)
                    {
                        State = FaunaBehaviourState.Despawn;
                        break;
                    }
                    Roam();
                    break;
                case FaunaBehaviourState.Despawn:
                    break;
            }
        }

        private void Roam()
        {
            int dx = (int)Math.Round(_rng.NextFloat2(-1f, 1f).x);
            int dy = (int)Math.Round(_rng.NextFloat2(-1f, 1f).y);
            Position += new int3(dx, dy, 0);
        }
    }

    /// <summary>Simple behaviour states for wildlife.</summary>
    public enum FaunaBehaviourState
    {
        Spawning,
        Roaming,
        Despawn
    }
}
