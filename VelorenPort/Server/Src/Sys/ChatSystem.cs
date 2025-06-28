using System;
using System.Collections.Generic;
using System.Linq;
using VelorenPort.CoreEngine.comp;
using VelorenPort.Network;
using VelorenPort.Server.Events;

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
        IEnumerable<Client> clients)
    {
        foreach (var ev in events.DrainChatEvents())
        {
            var msg = ev.Msg;
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
}
