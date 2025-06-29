using VelorenPort.NativeMath;
using VelorenPort.CoreEngine;

namespace VelorenPort.Server.Agent;

public static class NpcBehaviorFactory
{
    private const float DetectionRange = 10f;
    private const float AttackRange = 2f;
    private const float AttackDamage = 5f;
    private const float AttackCooldown = 1f;
    private const float ChaseSpeed = 3f;
    private const float WanderSpeed = 1f;

    public static BehaviorTree CreateDefault()
    {
        return new BehaviorTree(
            new SelectorNode(
                new SequenceNode(
                    new ActionNode(AcquireTarget),
                    new SelectorNode(
                        new SequenceNode(
                            new ActionNode(TargetInRange),
                            new ActionNode(Attack)
                        ),
                        new ActionNode(Chase)
                    )
                ),
                new ActionNode(Patrol)
            )
        );
    }

    private static BehaviorStatus AcquireTarget(EntityManager em, Entity entity, Npc npc, IEnumerable<Client> clients, float dt, BehaviorState state)
    {
        if (state.Target != null)
        {
            var dist = math.distance(state.Target.Position.Value, em.GetComponentData<Pos>(entity).Value);
            if (dist <= DetectionRange)
                return BehaviorStatus.Success;
        }

        Client? best = null;
        float bestDist = DetectionRange;
        var pos = em.GetComponentData<Pos>(entity).Value;
        foreach (var c in clients)
        {
            var d = math.distance(c.Position.Value, pos);
            if (d <= bestDist)
            {
                bestDist = d;
                best = c;
            }
        }
        state.Target = best;
        return best != null ? BehaviorStatus.Success : BehaviorStatus.Failure;
    }

    private static BehaviorStatus TargetInRange(EntityManager em, Entity entity, Npc npc, IEnumerable<Client> clients, float dt, BehaviorState state)
    {
        if (state.Target == null)
            return BehaviorStatus.Failure;
        var pos = em.GetComponentData<Pos>(entity).Value;
        var dist = math.distance(state.Target.Position.Value, pos);
        return dist <= AttackRange ? BehaviorStatus.Success : BehaviorStatus.Failure;
    }

    private static BehaviorStatus Attack(EntityManager em, Entity entity, Npc npc, IEnumerable<Client> clients, float dt, BehaviorState state)
    {
        if (state.Target == null)
            return BehaviorStatus.Failure;
        if (state.AttackCooldown > 0f)
            return BehaviorStatus.Running;
        npc.EnterCombat();
        CombatUtils.Apply(state.Target, new Attack(npc.Id, new HitInfo(state.Target.Uid, AttackDamage, DamageKind.Physical)), null);
        state.AttackCooldown = AttackCooldown;
        return BehaviorStatus.Success;
    }

    private static BehaviorStatus Chase(EntityManager em, Entity entity, Npc npc, IEnumerable<Client> clients, float dt, BehaviorState state)
    {
        if (state.Target == null)
            return BehaviorStatus.Failure;
        var pos = em.GetComponentData<Pos>(entity);
        var dir = math.normalize(state.Target.Position.Value - pos.Value);
        var vel = new Vel(dir * ChaseSpeed);
        pos = new Pos(pos.Value + vel.Value * dt);
        em.SetComponentData(entity, pos);
        if (em.HasComponent<Vel>(entity))
            em.SetComponentData(entity, vel);
        else
            em.AddComponentData(entity, vel);
        var yaw = math.atan2(dir.x, dir.z);
        var rot = math.axisAngle(new float3(0f, 1f, 0f), yaw);
        if (em.HasComponent<Ori>(entity))
            em.SetComponentData(entity, new Ori(rot));
        return BehaviorStatus.Running;
    }

    private static BehaviorStatus Patrol(EntityManager em, Entity entity, Npc npc, IEnumerable<Client> clients, float dt, BehaviorState state)
    {
        var pos = em.GetComponentData<Pos>(entity);
        if (state.PatrolTimer <= 0f)
        {
            var rand = new Random((uint)entity.Index + (uint)math.floor(pos.Value.x));
            state.WanderDir = rand.NextFloat2(-1f, 1f);
            if (math.lengthsq(state.WanderDir) > 0f)
                state.WanderDir = math.normalize(state.WanderDir);
            state.PatrolTimer = 2f;
        }
        var move = new float3(state.WanderDir.x, 0f, state.WanderDir.y) * WanderSpeed * dt;
        pos = new Pos(pos.Value + move);
        var vel = new Vel(move / dt);
        em.SetComponentData(entity, pos);
        if (em.HasComponent<Vel>(entity))
            em.SetComponentData(entity, vel);
        else
            em.AddComponentData(entity, vel);
        var yaw = math.atan2(state.WanderDir.x, state.WanderDir.y);
        var rot = math.axisAngle(new float3(0f, 1f, 0f), yaw);
        if (em.HasComponent<Ori>(entity))
            em.SetComponentData(entity, new Ori(rot));
        return BehaviorStatus.Running;
    }
}
