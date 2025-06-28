using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using VelorenPort.CoreEngine;
using VelorenPort.CoreEngine.comp;
using comp = VelorenPort.CoreEngine.comp;

namespace VelorenPort.Server
{
    /// <summary>
    /// Chat cache and exporter utilities. Mirrors <c>server/src/chat.rs</c>.
    /// </summary>
    public static class Chat
    {
        /// <summary>Information about a player for chat metadata.</summary>
        public sealed record PlayerInfo(Guid Uuid, string Alias);

        /// <summary>Represents the participants of a chat message.</summary>
        public abstract record ChatParties
        {
            public sealed record Online(PlayerInfo Player) : ChatParties;
            public sealed record Offline(PlayerInfo Player) : ChatParties;
            public sealed record CommandInfo(PlayerInfo Player) : ChatParties;
            public sealed record CommandError(PlayerInfo Player) : ChatParties;
            public sealed record Kill(KillSource Source, PlayerInfo Victim) : ChatParties;
            public sealed record GroupMeta(List<PlayerInfo> Members) : ChatParties;
            public sealed record Group(PlayerInfo From, List<PlayerInfo> Members) : ChatParties;
            public sealed record Tell(PlayerInfo From, PlayerInfo To) : ChatParties;
            public sealed record Say(PlayerInfo From) : ChatParties;
            public sealed record FactionMeta(string Faction) : ChatParties;
            public sealed record FactionMessage(PlayerInfo From, string Faction) : ChatParties;
            public sealed record Region(PlayerInfo From) : ChatParties;
            public sealed record World(PlayerInfo From) : ChatParties;
        }

        /// <summary>Reason a player was killed.</summary>
        public abstract record KillSource
        {
            public sealed record Player(PlayerInfo Info, BuffKind Kind) : KillSource;
            public sealed record NonPlayer(Content Content, BuffKind Kind) : KillSource;
            public sealed record NonExistent(BuffKind Kind) : KillSource;
            public sealed record FallDamage : KillSource;
            public sealed record Suicide : KillSource;
            public sealed record Other : KillSource;
        }

        /// <summary>Single chat entry stored in the cache.</summary>
        public sealed class ChatMessage
        {
            public DateTime Time { get; }
            public ChatParties Parties { get; }
            public Content Content { get; }

            private ChatMessage(DateTime time, ChatParties parties, Content content)
            {
                Time = time;
                Parties = parties;
                Content = content;
            }

            public static ChatMessage New(UnresolvedChatMsg msg, ChatParties parties)
                => new(DateTime.UtcNow, parties, msg.Content);
        }

        /// <summary>Allows other systems to send chat messages to the cache.</summary>
        public sealed class ChatExporter
        {
            private readonly ChannelWriter<ChatMessage> _writer;

            internal ChatExporter(ChannelWriter<ChatMessage> writer)
            {
                _writer = writer;
            }

            public void Send(ChatMessage msg)
            {
                _writer.TryWrite(msg);
            }

            private static KillSource? ConvertKillSource(comp.KillSource source, Func<Uid, PlayerInfo?> playerInfoFromUid)
            {
                return source switch
                {
                    comp.KillSource.Player p when playerInfoFromUid(p.Uid) is PlayerInfo info
                        => new KillSource.Player(info, p.Type),
                    comp.KillSource.NonPlayer np => new KillSource.NonPlayer(np.Content, np.Type),
                    comp.KillSource.NonExistent ne => new KillSource.NonExistent(ne.Type),
                    comp.KillSource.FallDamage => new KillSource.FallDamage(),
                    comp.KillSource.Suicide => new KillSource.Suicide(),
                    comp.KillSource.Other => new KillSource.Other(),
                    _ => null
                };
            }

            /// <summary>
            /// Generate a concrete chat message from an unresolved one using lookup callbacks.
            /// </summary>
            public ChatMessage? Generate(
                UnresolvedChatMsg chatmsg,
                Func<Uid, PlayerInfo?> playerInfoFromUid,
                Func<Group, IEnumerable<PlayerInfo>> groupMembersFromGroup)
            {
                ChatParties? parties = chatmsg.ChatType switch
                {
                    ChatType<Group>.Offline<Group> off when playerInfoFromUid(off.Uid) is PlayerInfo p
                        => new ChatParties.Offline(p),
                    ChatType<Group>.Online<Group> on when playerInfoFromUid(on.Uid) is PlayerInfo p
                        => new ChatParties.Online(p),
                    ChatType<Group>.Region<Group> r when playerInfoFromUid(r.Uid) is PlayerInfo p
                        => new ChatParties.Region(p),
                    ChatType<Group>.World<Group> w when playerInfoFromUid(w.Uid) is PlayerInfo p
                        => new ChatParties.World(p),
                    ChatType<Group>.Say<Group> s when playerInfoFromUid(s.Uid) is PlayerInfo p
                        => new ChatParties.Say(p),
                    ChatType<Group>.Tell<Group> t
                        when playerInfoFromUid(t.From) is PlayerInfo fp && playerInfoFromUid(t.To) is PlayerInfo tp
                        => new ChatParties.Tell(fp, tp),
                    ChatType<Group>.Kill<Group> k
                        when playerInfoFromUid(k.Victim) is PlayerInfo vp && ConvertKillSource(k.Source, playerInfoFromUid) is KillSource ks
                        => new ChatParties.Kill(ks, vp),
                    ChatType<Group>.FactionMeta<Group> fm
                        => new ChatParties.FactionMeta(fm.Faction),
                    ChatType<Group>.Faction<Group> f when playerInfoFromUid(f.From) is PlayerInfo fp2
                        => new ChatParties.FactionMessage(fp2, f.Faction),
                    ChatType<Group>.GroupMeta<Group> gm
                        => new ChatParties.GroupMeta(groupMembersFromGroup(gm.Group).ToList()),
                    ChatType<Group>.Group<Group> g when playerInfoFromUid(g.From) is PlayerInfo gp
                        => new ChatParties.Group(gp, groupMembersFromGroup(g.Group).ToList()),
                    _ => null
                };

                return parties == null ? null : ChatMessage.New(chatmsg, parties);
            }
        }

        /// <summary>Stores recent chat messages for retrieval.</summary>
        public sealed class ChatCache
        {
            private readonly List<ChatMessage> _messages;
            private readonly TimeSpan _keepDuration;
            private readonly ChannelReader<ChatMessage> _reader;
            private readonly Task _worker;
            private readonly object _lock = new();

            private ChatCache(List<ChatMessage> messages, ChannelReader<ChatMessage> reader, TimeSpan keepDuration)
            {
                _messages = messages;
                _reader = reader;
                _keepDuration = keepDuration;
                _worker = Task.Run(RunAsync);
            }

            private async Task RunAsync()
            {
                await foreach (var msg in _reader.ReadAllAsync())
                {
                    var dropOlderThan = msg.Time - _keepDuration;
                    lock (_lock)
                    {
                        while (_messages.Count > 0 && _messages[0].Time < dropOlderThan)
                            _messages.RemoveAt(0);
                        _messages.Add(msg);
                        const int MAX_CACHE_MESSAGES = 10_000;
                        if (_messages.Capacity > _messages.Count + MAX_CACHE_MESSAGES)
                            _messages.Capacity = _messages.Count;
                    }
                }
            }

            public static (ChatCache Cache, ChatExporter Exporter) Create(TimeSpan keepDuration)
            {
                const int BufferSize = 1_000;
                var channel = Channel.CreateBounded<ChatMessage>(BufferSize);
                var messages = new List<ChatMessage>();
                var cache = new ChatCache(messages, channel.Reader, keepDuration);
                var exporter = new ChatExporter(channel.Writer);
                return (cache, exporter);
            }

            public IReadOnlyList<ChatMessage> Messages
            {
                get { lock (_lock) { return _messages.ToList(); } }
            }
        }
    }
}
