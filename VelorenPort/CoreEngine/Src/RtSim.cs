using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Minimal subset of the rtsim module providing basic AI related
    /// structures used elsewhere in the port. This does not aim to
    /// mirror the full functionality of the original Rust module but
    /// includes the core data types so other systems can compile.
    /// </summary>


    /// <summary>
    /// Cardinal direction used by certain activities. This is a very
    /// small subset of the Rust <c>Dir</c> type and exists only so the
    /// controller APIs map more closely to the original.</summary>
    [Serializable]
    public enum Dir { North, East, South, West }

    /// <summary>Flight behaviour for flying NPCs.</summary>
    [Serializable]
    public enum FlightMode { Braking, FlyThrough }

    /// <summary>High level activity an NPC is currently performing.</summary>
    [Serializable]
    public abstract record NpcActivity {
        [Serializable]
        public sealed record Goto(float3 Position, float Speed) : NpcActivity;

        [Serializable]
        public sealed record GotoFlying(float3 Position, float Speed,
                                        float? Height, Dir? Dir,
                                        FlightMode Mode) : NpcActivity;

        [Serializable]
        public sealed record Gather(ChunkResource[] Resources) : NpcActivity;

        [Serializable]
        public sealed record HuntAnimals : NpcActivity;

        [Serializable]
        public sealed record Dance(Dir? Facing) : NpcActivity;

        [Serializable]
        public sealed record Cheer(Dir? Facing) : NpcActivity;

        [Serializable]
        public sealed record Sit(Dir? Facing, int3? Position) : NpcActivity;

        [Serializable]
        public sealed record Talk(Actor Target) : NpcActivity;
    }

    /// <summary>Simple message structure used by <see cref="NpcAction"/>.</summary>
    [Serializable]
    public struct Dialogue {
        public DialogueId Id;
        public DialogueKind Kind;
    }

    [Serializable]
    public struct DialogueId {
        public ulong Value;
        public DialogueId(ulong value) { Value = value; }
        public static implicit operator ulong(DialogueId id) => id.Value;
        public static implicit operator DialogueId(ulong value) => new(value);
    }

    /// <summary>Variants for <see cref="Dialogue"/>.</summary>
    [Serializable]
    public abstract record DialogueKind {
        [Serializable] public sealed record Start : DialogueKind;
        [Serializable] public sealed record End : DialogueKind;
        [Serializable] public sealed record Statement(Content Msg) : DialogueKind;

        [Serializable]
        public sealed record Question(uint Tag, Content Msg,
            Dictionary<ushort, Response> Responses) : DialogueKind;

        [Serializable]
        public sealed record ResponseOption(uint Tag, Response Response, ushort ResponseId) : DialogueKind;

        [Serializable]
        public sealed record Marker(int2 Wpos, Content Name) : DialogueKind;
    }

    [Serializable]
    public struct Response {
        public Content Msg;
        public Response(Content msg) { Msg = msg; }
    }

    /// <summary>Represents an ongoing dialogue with another actor.</summary>
    [Serializable]
    public struct DialogueSession {
        public Actor Target;
        public DialogueId Id;
        public DialogueSession(Actor target, DialogueId id) {
            Target = target;
            Id = id;
        }
    }

    [Serializable]
    public abstract record NpcAction {
        [Serializable]
        public sealed record Say(Actor? Target, Content Msg) : NpcAction;

        [Serializable]
        public sealed record Attack(Actor Target) : NpcAction;

        [Serializable]
        public sealed record Dialogue(Actor Target, global::VelorenPort.CoreEngine.Dialogue Message) : NpcAction;
    }

    [Serializable]
    public abstract record NpcInput {
        [Serializable]
        public sealed record Report(uint ReportId) : NpcInput;
        [Serializable]
        public sealed record Interaction(Actor Target) : NpcInput;
        [Serializable]
        public sealed record Dialogue(Actor Target, global::VelorenPort.CoreEngine.Dialogue Message) : NpcInput;
    }

    /// <summary>Traits describing an NPC's personality.</summary>
    public enum PersonalityTrait {
        Open,
        Adventurous,
        Closed,
        Conscientious,
        Busybody,
        Unconscientious,
        Extroverted,
        Introverted,
        Agreeable,
        Sociable,
        Disagreeable,
        Neurotic,
        Seeker,
        Worried,
        SadLoner,
        Stable,
    }

    /// <summary>
    /// Basic personality stats. Only implements simple random generation and
    /// trait checks required by tests and other modules.
    /// </summary>
    [Serializable]
    public struct Personality {
        public byte Openness;
        public byte Conscientiousness;
        public byte Extraversion;
        public byte Agreeableness;
        public byte Neuroticism;

        public static Personality Random(System.Random rng) {
            return new Personality {
                Openness = (byte)rng.Next(0, 256),
                Conscientiousness = (byte)rng.Next(0, 256),
                Extraversion = (byte)rng.Next(0, 256),
                Agreeableness = (byte)rng.Next(0, 256),
                Neuroticism = (byte)rng.Next(0, 256),
            };
        }

        public static Personality RandomGood(System.Random rng) {
            return new Personality {
                Openness = (byte)rng.Next(0, 256),
                Conscientiousness = (byte)rng.Next(128, 256),
                Extraversion = (byte)rng.Next(0, 256),
                Agreeableness = (byte)rng.Next(128, 256),
                Neuroticism = (byte)rng.Next(0, 256),
            };
        }

        public static Personality RandomEvil(System.Random rng) {
            return new Personality {
                Openness = (byte)rng.Next(0, 256),
                Conscientiousness = (byte)rng.Next(0, 128),
                Extraversion = (byte)rng.Next(0, 256),
                Agreeableness = (byte)rng.Next(0, 128),
                Neuroticism = (byte)rng.Next(0, 256),
            };
        }

        public bool Is(PersonalityTrait trait) => trait switch {
            PersonalityTrait.Open => Openness > 200,
            PersonalityTrait.Adventurous => Openness > 200 && Neuroticism < 128,
            PersonalityTrait.Closed => Openness < 55,
            PersonalityTrait.Conscientious => Conscientiousness > 200,
            PersonalityTrait.Busybody => Agreeableness < 55,
            PersonalityTrait.Unconscientious => Conscientiousness < 55,
            PersonalityTrait.Extroverted => Extraversion > 200,
            PersonalityTrait.Introverted => Extraversion < 55,
            PersonalityTrait.Agreeable => Agreeableness > 200,
            PersonalityTrait.Sociable => Agreeableness > 200 && Extraversion > 128,
            PersonalityTrait.Disagreeable => Agreeableness < 55,
            PersonalityTrait.Neurotic => Neuroticism > 200,
            PersonalityTrait.Seeker => Neuroticism > 200 && Openness > 180,
            PersonalityTrait.Worried => Neuroticism > 200 && Agreeableness > 180,
            PersonalityTrait.SadLoner => Neuroticism > 200 && Extraversion < 75,
            PersonalityTrait.Stable => Neuroticism < 55,
            _ => false,
        };
    }

    [Serializable]
    public enum Role {
        Civilised,
        Wild,
        Monster,
        Vehicle,
    }

    [Serializable]
    public enum Profession {
        Farmer,
        Hunter,
        Merchant,
        Guard,
        Adventurer,
        Blacksmith,
        Chef,
        Alchemist,
        Pirate,
        Cultist,
        Herbalist,
        Captain,
    }

    [Serializable]
    public struct WorldSettings {
        public double StartTime;
        public static WorldSettings Default => new WorldSettings { StartTime = 9.0 * 3600.0 };
    }

    /// <summary>
    /// Controller used by the rtsim system to direct NPC behaviour. This is only
    /// a lightweight stand-in for the much richer controller found in the Rust
    /// code. It stores the current activity and queued actions.
    /// </summary>
    [Serializable]
    public class RtSimController {
        public NpcActivity? Activity { get; private set; }
        public Queue<NpcAction> Actions { get; } = new();
        public Personality Personality = Personality.Random(new System.Random());
        public string? HeadingTo { get; set; }
        public float3? LookDir { get; set; }

        public void Reset() {
            Activity = null;
            LookDir = null;
        }

        public void DoIdle() => Activity = null;

        public void DoTalk(Actor target) => Activity = new NpcActivity.Talk(target);

        public void DoGoto(float3 pos, float speed = 0.5f) =>
            Activity = new NpcActivity.Goto(pos, speed);

        public void DoGotoFlying(float3 pos, float speed, float? height = null,
                                  Dir? dir = null, FlightMode mode = FlightMode.Braking) =>
            Activity = new NpcActivity.GotoFlying(pos, speed, height, dir, mode);

        public void DoGather(params ChunkResource[] res) =>
            Activity = new NpcActivity.Gather(res);

        public void DoHuntAnimals() => Activity = new NpcActivity.HuntAnimals();

        public void DoDance(Dir? dir = null) => Activity = new NpcActivity.Dance(dir);

        public void DoCheer(Dir? dir = null) => Activity = new NpcActivity.Cheer(dir);

        public void DoSit(Dir? dir = null, int3? pos = null) =>
            Activity = new NpcActivity.Sit(dir, pos);

        public void Say(Actor? target, Content msg) =>
            Actions.Enqueue(new NpcAction.Say(target, msg));

        public void Attack(Actor target) =>
            Actions.Enqueue(new NpcAction.Attack(target));

        public DialogueSession DialogueStart(Actor target) {
            var id = new DialogueId((ulong)System.Random.Shared.NextInt64());
            var session = new DialogueSession(target, id);
            Actions.Enqueue(new NpcAction.Dialogue(target,
                new Dialogue { Id = id, Kind = new DialogueKind.Start() }));
            return session;
        }

        public void DialogueEnd(DialogueSession session) {
            Actions.Enqueue(new NpcAction.Dialogue(session.Target,
                new Dialogue { Id = session.Id, Kind = new DialogueKind.End() }));
        }

        public uint DialogueQuestion(DialogueSession session, Content msg,
            IEnumerable<(ushort Id, Response Response)> responses) {
            uint tag = (uint)System.Random.Shared.Next();
            var map = new Dictionary<ushort, Response>();
            foreach (var (id, resp) in responses)
                map[id] = resp;
            Actions.Enqueue(new NpcAction.Dialogue(session.Target,
                new Dialogue {
                    Id = session.Id,
                    Kind = new DialogueKind.Question(tag, msg, map)
                }));
            return tag;
        }

        public void DialogueStatement(DialogueSession session, Content msg) {
            Actions.Enqueue(new NpcAction.Dialogue(session.Target,
                new Dialogue { Id = session.Id, Kind = new DialogueKind.Statement(msg) }));
        }

        public void DialogueMarker(DialogueSession session, int2 pos, Content name) {
            Actions.Enqueue(new NpcAction.Dialogue(session.Target,
                new Dialogue { Id = session.Id, Kind = new DialogueKind.Marker(pos, name) }));
        }

        public static RtSimController WithDestination(float3 pos) {
            var ctrl = new RtSimController();
            ctrl.DoGoto(pos);
            return ctrl;
        }
    }
}
