using System;
using System.Linq;
using VelorenPort.CoreEngine.comp;
using VelorenPort.Network;

namespace VelorenPort.Server
{
    /// <summary>
    /// Simplified invite handling used by <see cref="GameServer"/>. It allows
    /// players to send basic group or trade invites to each other and tracks
    /// pending invites for timeout processing.
    /// </summary>
    public class InviteManager
    {
        private readonly GameServer _server;
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(30);

        public InviteManager(GameServer server)
        {
            _server = server;
        }

        /// <summary>
        /// Send an invite from one client to another. Fails silently if either
        /// client cannot be found or an invite already exists.
        /// </summary>
        public void SendInvite(Uid inviterUid, Uid inviteeUid, InviteKind kind)
        {
            if (inviterUid.Equals(inviteeUid))
                return;

            var inviter = _server.Clients.FirstOrDefault(c => c.Uid.Equals(inviterUid));
            var invitee = _server.Clients.FirstOrDefault(c => c.Uid.Equals(inviteeUid));
            if (inviter == null || invitee == null)
                return;

            if (inviter.PendingInvites.Invites.Any(i => i.Invitee.Equals(inviteeUid)))
                return;

            var expires = DateTime.UtcNow + Timeout;
            inviter.PendingInvites.Add(inviteeUid, kind, expires);

            var inviteMsg = PreparedMsg.Create(
                0,
                new ServerGeneral.Invite(inviterUid, Timeout, kind),
                new StreamParams(Promises.Ordered));
            invitee.SendPreparedAsync(inviteMsg).GetAwaiter().GetResult();

            var pendingMsg = PreparedMsg.Create(
                0,
                new ServerGeneral.InvitePending(inviteeUid),
                new StreamParams(Promises.Ordered));
            inviter.SendPreparedAsync(pendingMsg).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Process an answer to a previously sent invite.
        /// </summary>
        public void HandleResponse(Uid inviteeUid, Uid inviterUid, InviteKind kind, InviteAnswer answer)
        {
            var inviter = _server.Clients.FirstOrDefault(c => c.Uid.Equals(inviterUid));
            if (inviter == null)
                return;

            var pending = inviter.PendingInvites.Invites;
            for (int i = 0; i < pending.Count; i++)
            {
                var entry = pending[i];
                if (entry.Invitee.Equals(inviteeUid) && entry.Kind == kind)
                {
                    pending.RemoveAt(i);
                    break;
                }
            }

            var msg = PreparedMsg.Create(
                0,
                new ServerGeneral.InviteComplete(inviteeUid, answer, kind),
                new StreamParams(Promises.Ordered));
            inviter.SendPreparedAsync(msg).GetAwaiter().GetResult();

            if (answer == InviteAnswer.Accepted && kind == InviteKind.Group)
                _server.GroupManager.JoinGroup(inviterUid, inviteeUid);
        }
    }
}
