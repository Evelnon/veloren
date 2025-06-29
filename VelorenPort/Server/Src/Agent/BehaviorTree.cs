using System;
using System.Collections.Generic;
using VelorenPort.NativeMath;
using VelorenPort.CoreEngine;

namespace VelorenPort.Server.Agent;

public enum BehaviorStatus
{
    Success,
    Failure,
    Running
}

public interface IBehaviorNode
{
    BehaviorStatus Tick(EntityManager em, Entity entity, Npc npc, IEnumerable<Client> clients, float dt, BehaviorState state);
}

public sealed class BehaviorState
{
    public Client? Target;
    public float AttackCooldown;
    public float PatrolTimer;
    public float2 WanderDir;
}

public sealed class BehaviorTree
{
    private readonly IBehaviorNode _root;
    private readonly BehaviorState _state = new();

    public BehaviorTree(IBehaviorNode root)
    {
        _root = root;
    }

    public void Tick(EntityManager em, Entity entity, Npc npc, IEnumerable<Client> clients, float dt)
    {
        if (_state.AttackCooldown > 0f)
            _state.AttackCooldown -= dt;
        if (_state.PatrolTimer > 0f)
            _state.PatrolTimer -= dt;
        _root.Tick(em, entity, npc, clients, dt, _state);
    }
}

public sealed class SequenceNode : IBehaviorNode
{
    private readonly IBehaviorNode[] _children;

    public SequenceNode(params IBehaviorNode[] children)
    {
        _children = children;
    }

    public BehaviorStatus Tick(EntityManager em, Entity entity, Npc npc, IEnumerable<Client> clients, float dt, BehaviorState state)
    {
        foreach (var child in _children)
        {
            var result = child.Tick(em, entity, npc, clients, dt, state);
            if (result != BehaviorStatus.Success)
                return result;
        }
        return BehaviorStatus.Success;
    }
}

public sealed class SelectorNode : IBehaviorNode
{
    private readonly IBehaviorNode[] _children;

    public SelectorNode(params IBehaviorNode[] children)
    {
        _children = children;
    }

    public BehaviorStatus Tick(EntityManager em, Entity entity, Npc npc, IEnumerable<Client> clients, float dt, BehaviorState state)
    {
        foreach (var child in _children)
        {
            var result = child.Tick(em, entity, npc, clients, dt, state);
            if (result == BehaviorStatus.Success)
                return BehaviorStatus.Success;
            if (result == BehaviorStatus.Running)
                return BehaviorStatus.Running;
        }
        return BehaviorStatus.Failure;
    }
}

public sealed class ActionNode : IBehaviorNode
{
    private readonly Func<EntityManager, Entity, Npc, IEnumerable<Client>, float, BehaviorState, BehaviorStatus> _action;

    public ActionNode(Func<EntityManager, Entity, Npc, IEnumerable<Client>, float, BehaviorState, BehaviorStatus> action)
    {
        _action = action;
    }

    public BehaviorStatus Tick(EntityManager em, Entity entity, Npc npc, IEnumerable<Client> clients, float dt, BehaviorState state)
        => _action(em, entity, npc, clients, dt, state);
}
