using System;
using System.Collections.Generic;
using System.Linq;
using VelorenPort.CoreEngine.comp;
using VelorenPort.Network;

namespace VelorenPort.Server.Sys;

/// <summary>
/// Removes timed out invites from pending lists and notifies the inviter.
/// This is a greatly simplified version of <c>invite_timeout.rs</c>.
/// </summary>
public static class InviteTimeout
{
    public static void Update(IEnumerable<Client> clients)
    {
        var now = DateTime.UtcNow;
        foreach (var inviter in clients)
        {
            var pending = inviter.PendingInvites.Invites;
            for (int i = pending.Count - 1; i >= 0; i--)
            {
                var (invitee, kind, timeout) = pending[i];
                if (timeout > now) continue;

                pending.RemoveAt(i);

                var target = clients.FirstOrDefault(c => c.Uid.Equals(invitee));
                if (target != null)
                {
                    var msg = PreparedMsg.Create(
                        0,
                        new ServerGeneral.InviteComplete(target.Uid, InviteAnswer.TimedOut, kind),
                        new StreamParams(Promises.Ordered));
                    inviter.SendPreparedAsync(msg).GetAwaiter().GetResult();
                }
            }
        }
    }
}
