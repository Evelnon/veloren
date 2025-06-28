using System;

namespace VelorenPort.CoreEngine.comp {
    using VelorenPort.CoreEngine;

    /// <summary>
    /// Player chat modes. Mirrors `ChatMode` from `common/src/comp/chat.rs`.
    /// </summary>
    [Serializable]
    public abstract record ChatMode {
        [Serializable]
        public sealed record Tell(Uid To) : ChatMode;
        [Serializable]
        public sealed record Say : ChatMode;
        [Serializable]
        public sealed record Region : ChatMode;
        [Serializable]
        public sealed record Group : ChatMode;
        [Serializable]
        public sealed record Faction(string Name) : ChatMode;
        [Serializable]
        public sealed record World : ChatMode;

        public UnresolvedChatMsg ToMsg(Uid from, Content content, comp.Group? group) {
            ChatType<comp.Group> chatType = this switch {
                Tell t => new ChatType<comp.Group>.Tell<comp.Group>(from, t.To),
                Say => new ChatType<comp.Group>.Say<comp.Group>(from),
                Region => new ChatType<comp.Group>.Region<comp.Group>(from),
                Group => new ChatType<comp.Group>.Group<comp.Group>(from, group ?? comp.Group.ENEMY),
                Faction f => new ChatType<comp.Group>.Faction<comp.Group>(from, f.Name),
                World => new ChatType<comp.Group>.World<comp.Group>(from),
                _ => throw new ArgumentOutOfRangeException()
            };
            return new UnresolvedChatMsg(chatType, content);
        }

        public static ChatMode Default => new World();
    }

    [Serializable]
    public abstract record KillType {
        [Serializable] public sealed record Buff(BuffKind Kind) : KillType;
        [Serializable] public sealed record Melee : KillType;
        [Serializable] public sealed record Projectile : KillType;
        [Serializable] public sealed record Explosion : KillType;
        [Serializable] public sealed record Energy : KillType;
        [Serializable] public sealed record Other : KillType;
    }

    [Serializable]
    public abstract record KillSource {
        [Serializable] public sealed record Player(Uid Uid, KillType Type) : KillSource;
        [Serializable] public sealed record NonPlayer(Content Content, KillType Type) : KillSource;
        [Serializable] public sealed record NonExistent(KillType Type) : KillSource;
        [Serializable] public sealed record FallDamage : KillSource;
        [Serializable] public sealed record Suicide : KillSource;
        [Serializable] public sealed record Other : KillSource;
    }

    [Serializable]
    public abstract record ChatType<G> {
        [Serializable] public sealed record Online<U>(Uid UserId) : ChatType<U>;
        [Serializable] public sealed record Offline<U>(Uid UserId) : ChatType<U>;
        [Serializable] public sealed record CommandInfo<U> : ChatType<U>;
        [Serializable] public sealed record CommandError<U> : ChatType<U>;
        [Serializable] public sealed record Kill<U>(KillSource Source, Uid Victim) : ChatType<U>;
        [Serializable] public sealed record GroupMeta<U>(U Group) : ChatType<U>;
        [Serializable] public sealed record FactionMeta<U>(string Faction) : ChatType<U>;
        [Serializable] public sealed record Tell<U>(Uid From, Uid To) : ChatType<U>;
        [Serializable] public sealed record Say<U>(Uid From) : ChatType<U>;
        [Serializable] public sealed record Group<U>(Uid From, U GroupData) : ChatType<U>;
        [Serializable] public sealed record Faction<U>(Uid From, string FactionName) : ChatType<U>;
        [Serializable] public sealed record Region<U>(Uid From) : ChatType<U>;
        [Serializable] public sealed record World<U>(Uid From) : ChatType<U>;
        [Serializable] public sealed record Npc<U>(Uid From) : ChatType<U>;
        [Serializable] public sealed record NpcSay<U>(Uid From) : ChatType<U>;
        [Serializable] public sealed record NpcTell<U>(Uid From, Uid To) : ChatType<U>;
        [Serializable] public sealed record Meta<U> : ChatType<U>;

        public static bool? IsPrivate(ChatType<G> ct) => ct switch {
            Online<G> => null,
            Offline<G> => null,
            CommandInfo<G> => null,
            CommandError<G> => null,
            FactionMeta<G> => null,
            GroupMeta<G> => null,
            Npc<G> => null,
            NpcSay<G> => null,
            NpcTell<G> => null,
            Meta<G> => null,
            Kill<G> => null,
            Tell<G> => true,
            Group<G> => true,
            Faction<G> => true,
            Say<G> => false,
            Region<G> => false,
            World<G> => false,
            _ => null
        };

        public static Uid? Uid(ChatType<G> ct) => ct switch {
            Tell<G> t => t.From,
            Say<G> s => s.From,
            Group<G> g => g.From,
            Faction<G> f => f.From,
            Region<G> r => r.From,
            World<G> w => w.From,
            Npc<G> n => n.From,
            NpcSay<G> n => n.From,
            NpcTell<G> n => n.From,
            _ => null
        };

        public GenericChatMsg<G> IntoPlainMsg(string text) =>
            new GenericChatMsg<G>(this, new Content.Plain(text));

        public GenericChatMsg<G> IntoMsg(Content content) =>
            new GenericChatMsg<G>(this, content);
    }

    [Serializable]
    public class GenericChatMsg<G> {
        public ChatType<G> ChatType { get; set; }
        public Content Content { get; set; }

        public const int MAX_BYTES_PLAYER_CHAT_MSG = 256;
        public const float NPC_DISTANCE = 100f;
        public const float NPC_SAY_DISTANCE = 30f;
        public const float REGION_DISTANCE = 1000f;
        public const float SAY_DISTANCE = 100f;

        public GenericChatMsg(ChatType<G> chatType, Content content) {
            ChatType = chatType;
            Content = content;
        }

        public static GenericChatMsg<G> Npc(Uid uid, Content content) =>
            new(new ChatType<G>.Npc<G>(uid), content);

        public static GenericChatMsg<G> NpcSay(Uid uid, Content content) =>
            new(new ChatType<G>.NpcSay<G>(uid), content);

        public static GenericChatMsg<G> NpcTell(Uid from, Uid to, Content content) =>
            new(new ChatType<G>.NpcTell<G>(from, to), content);

        public static GenericChatMsg<G> Death(Uid victim, KillSource source) =>
            new(new ChatType<G>.Kill<G>(source, victim), new Content.Plain(string.Empty));

        public GenericChatMsg<T> MapGroup<T>(Func<G, T> f)
        {
            ChatType<T> chatType = ChatType switch
            {
                ChatType<G>.Online<G> on => new ChatType<T>.Online<T>(on.UserId),
                ChatType<G>.Offline<G> off => new ChatType<T>.Offline<T>(off.UserId),
                ChatType<G>.CommandInfo<G> => new ChatType<T>.CommandInfo<T>(),
                ChatType<G>.CommandError<G> => new ChatType<T>.CommandError<T>(),
                ChatType<G>.Kill<G> k => new ChatType<T>.Kill<T>(k.Source, k.Victim),
                ChatType<G>.GroupMeta<G> gm => new ChatType<T>.GroupMeta<T>(f(gm.Group)),
                ChatType<G>.FactionMeta<G> fm => new ChatType<T>.FactionMeta<T>(fm.Faction),
                ChatType<G>.Tell<G> t => new ChatType<T>.Tell<T>(t.From, t.To),
                ChatType<G>.Say<G> s => new ChatType<T>.Say<T>(s.From),
                ChatType<G>.Group<G> g => new ChatType<T>.Group<T>(g.From, f(g.GroupData)),
                ChatType<G>.Faction<G> fac => new ChatType<T>.Faction<T>(fac.From, fac.FactionName),
                ChatType<G>.Region<G> r => new ChatType<T>.Region<T>(r.From),
                ChatType<G>.World<G> w => new ChatType<T>.World<T>(w.From),
                ChatType<G>.Npc<G> n => new ChatType<T>.Npc<T>(n.From),
                ChatType<G>.NpcSay<G> n => new ChatType<T>.NpcSay<T>(n.From),
                ChatType<G>.NpcTell<G> n => new ChatType<T>.NpcTell<T>(n.From, n.To),
                ChatType<G>.Meta<G> => new ChatType<T>.Meta<T>(),
                _ => throw new ArgumentOutOfRangeException()
            };

            return new GenericChatMsg<T>(chatType, Content);
        }

        public G? GetGroup() => ChatType switch
        {
            ChatType<G>.GroupMeta<G> gm => gm.Group,
            ChatType<G>.Group<G> g => g.GroupData,
            _ => default
        };

        public (SpeechBubble Bubble, Uid From)? ToBubble()
        {
            var uid = ChatType<G>.Uid(ChatType);
            return uid.HasValue ? (new SpeechBubble(Content, Icon()), uid.Value) : null;
        }

        public SpeechBubbleType Icon() => ChatType switch
        {
            ChatType<G>.Tell<G> => SpeechBubbleType.Tell,
            ChatType<G>.Say<G> => SpeechBubbleType.Say,
            ChatType<G>.Group<G> => SpeechBubbleType.Group,
            ChatType<G>.Faction<G> => SpeechBubbleType.Faction,
            ChatType<G>.Region<G> => SpeechBubbleType.Region,
            ChatType<G>.World<G> => SpeechBubbleType.World,
            ChatType<G>.NpcSay<G> => SpeechBubbleType.Say,
            ChatType<G>.NpcTell<G> => SpeechBubbleType.Say,
            _ => SpeechBubbleType.None
        };

        public Uid? Uid() => ChatType<G>.Uid(ChatType);

        public Content GetContent() => Content;

        public Content IntoContent() => Content;

        public void SetContent(Content content) => Content = content;
    }

    public sealed record Faction(string Name);

    public enum SpeechBubbleType {
        Tell,
        Say,
        Region,
        Group,
        Faction,
        World,
        Quest,
        Trade,
        None,
    }

    public class SpeechBubble {
        public const double DEFAULT_DURATION = 5.0;
        public Content Content { get; }
        public SpeechBubbleType Icon { get; }
        public DateTime Timeout { get; }

        public SpeechBubble(Content content, SpeechBubbleType icon) {
            Content = content;
            Icon = icon;
            Timeout = DateTime.UtcNow + TimeSpan.FromSeconds(DEFAULT_DURATION);
        }
    }

    public record ChatMsg(ChatType<string> ChatType, Content Content);
    public record UnresolvedChatMsg(ChatType<Group> ChatType, Content Content);
}
