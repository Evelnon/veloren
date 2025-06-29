using System;
using System.Collections.Generic;
using System.Linq;
using VelorenPort.CoreEngine.comp;
using VelorenPort.Network;
using VelorenPort.Server.Events;
using VelorenPort.Server;

namespace VelorenPort.Server.Sys;

/// <summary>
/// Processes chat events, stores them in the chat cache and broadcasts
/// them to all connected clients. This mirrors a tiny portion of the
/// Rust chat system.
/// </summary>
public static class ChatSystem
{
    public static void Update(
        EventManager events,
        Chat.ChatExporter exporter,
        AutoMod automod,
        IEnumerable<Client> clients)
    {
        var clientMap = clients.ToDictionary(c => c.Uid);
        foreach (var ev in events.DrainChatEvents())
        {
            var msg = ev.Msg;
            if (ev.FromClient && ChatType<Group>.Uid(msg.ChatType) is Uid uid &&
                clientMap.TryGetValue(uid, out var sender))
            {
                var result = automod.ValidateChatMsg(
                    GuidFromUid(uid),
                    null,
                    DateTime.UtcNow,
                    msg.ChatType,
                    msg.Content.AsPlain() ?? string.Empty);
                if (!result.IsOk)
                {
                    SendSystemMessage(sender, result.Error!.ToString(), true);
                    continue;
                }
                if (result.Note == ActionNote.SpamWarn)
                    SendSystemMessage(sender, "Please slow down with the chat.", false);
            }

            var exported = exporter.Generate(
                msg,
                uid => new Chat.PlayerInfo(Guid.Empty, uid.Value.ToString()),
                _ => Enumerable.Empty<Chat.PlayerInfo>());
            if (exported != null)
                exporter.Send(exported);

            var networkMsg = PreparedMsg.Create(
                0,
                new ServerGeneral.ChatMsg(msg.MapGroup(g => g.ToString())),
                new StreamParams(Promises.Ordered));
            foreach (var c in clients)
                c.SendPreparedAsync(networkMsg).GetAwaiter().GetResult();
        }
    }

    private static void SendSystemMessage(Client client, string text, bool error)
    {
        var chat = new ChatMsg(
            error ? new ChatType<string>.CommandError<string>()
                  : new ChatType<string>.CommandInfo<string>(),
            new Content.Plain(text));
        var msg = PreparedMsg.Create(
            0,
            new ServerGeneral.ChatMsg(chat),
            new StreamParams(Promises.Ordered));
        client.SendPreparedAsync(msg).GetAwaiter().GetResult();
    }

    private static Guid GuidFromUid(Uid uid)
    {
        var bytes = new byte[16];
        BitConverter.GetBytes(uid.Value).CopyTo(bytes, 0);
        return new Guid(bytes);
    }
}
