using System;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Basic actions that one entity can perform on another.
    /// </summary>
    [Serializable]
    public enum InteractAction {
        Talk,
        Use,
        Loot,
        Trade
    }

    /// <summary>
    /// Simple event representing an interaction.
    /// </summary>
    [Serializable]
    public struct InteractionEvent {
        public Uid Actor;
        public Uid Target;
        public InteractAction Action;

        public InteractionEvent(Uid actor, Uid target, InteractAction action) {
            Actor = actor;
            Target = target;
            Action = action;
        }

        public static InteractionEvent Make(Uid actor, Uid target, InteractAction action)
            => new InteractionEvent(actor, target, action);

        public override string ToString() => $"{Actor} {Action} {Target}";
    }

    /// <summary>Lightweight event queue for interactions.</summary>
    public class InteractionBus {
        private readonly System.Collections.Generic.Queue<InteractionEvent> _evts = new();

        public void Publish(in InteractionEvent evt) => _evts.Enqueue(evt);

        public bool TryDequeue(out InteractionEvent evt) => _evts.TryDequeue(out evt);

        public int Count => _evts.Count;
    }

    /// <summary>Interface implemented by entities that can be interacted with.</summary>
    public interface IInteractable {
        Uid Id { get; }
        void OnInteract(Uid actor, InteractAction action);
    }

    /// <summary>
    /// Simple interaction system that dequeues events from an
    /// <see cref="InteractionBus"/> and dispatches them to their targets.
    /// </summary>
    public class InteractionSystem {
        private readonly InteractionBus _bus;
        private readonly System.Collections.Generic.Dictionary<Uid, IInteractable> _targets = new();

        public InteractionSystem(InteractionBus bus) { _bus = bus; }

        public void Register(IInteractable target) => _targets[target.Id] = target;

        public void Unregister(Uid id) => _targets.Remove(id);

        public void Update() {
            while (_bus.TryDequeue(out var evt)) {
                if (_targets.TryGetValue(evt.Target, out var t))
                    t.OnInteract(evt.Actor, evt.Action);
            }
        }
    }
}
