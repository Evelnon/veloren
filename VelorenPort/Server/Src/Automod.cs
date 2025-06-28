using System;
using System.Collections.Generic;

using VelorenPort.CoreEngine.comp;
using VelorenPort.CoreEngine;

namespace VelorenPort.Server {
    public enum ActionNote {
        SpamWarn,
    }

    public enum ActionErrType {
        BannedWord,
        TooLong,
        SpamMuted,
    }

    public readonly struct ActionErr {
        public ActionErrType Type { get; }
        public TimeSpan? Duration { get; }
        public ActionErr(ActionErrType type, TimeSpan? duration = null) {
            Type = type; Duration = duration;
        }
        public override string ToString() => Type switch {
            ActionErrType.BannedWord => "Your message contained a banned word. If you think this is a mistake, please let a moderator know.",
            ActionErrType.TooLong => $"Your message was too long, no more than {GenericChatMsg<string>.MAX_BYTES_PLAYER_CHAT_MSG} characters are permitted.",
            ActionErrType.SpamMuted => $"You have sent too many messages and are muted for {Duration?.TotalSeconds:F0} seconds.",
            _ => string.Empty,
        };
    }

    public class Censor {
        private readonly HashSet<string> _banned = new();
        public Censor(IEnumerable<string> bannedWords) {
            foreach (var w in bannedWords) _banned.Add(w.ToLowerInvariant());
        }
        public bool Check(string msg) {
            var lower = msg.ToLowerInvariant();
            foreach (var w in _banned) if (lower.Contains(w)) return true;
            return false;
        }
    }

    public class AutoMod {
        private readonly ModerationSettings _settings;
        private readonly Censor _censor;
        private readonly Dictionary<Guid, PlayerState> _players = new();

        public AutoMod(ModerationSettings settings, Censor censor) {
            _settings = settings;
            _censor = censor;
            if (_settings.Automod) {
                UnityEngine.Debug.Log($"Automod enabled, players{(_settings.AdminsExempt ? string.Empty : " (and admins)")} will be subject to automated spam/content filters");
            } else {
                UnityEngine.Debug.Log("Automod disabled");
            }
        }

        public bool Enabled => _settings.Automod;

        private PlayerState Player(Guid id) {
            if (!_players.TryGetValue(id, out var p)) {
                p = new PlayerState();
                _players[id] = p;
            }
            return p;
        }

        public Result ValidateChatMsg(Guid player, AdminRole? role, DateTime now, ChatType<Group> chatType, string msg) {
            if (msg.Length > GenericChatMsg<string>.MAX_BYTES_PLAYER_CHAT_MSG) {
                return Result.Err(new ActionErr(ActionErrType.TooLong));
            }
            if (!_settings.Automod || chatType switch {
                    var t when ChatType<Group>.IsPrivate(t) == true => true,
                    _ => false
                } || (role.HasValue && _settings.AdminsExempt)) {
                return Result.Ok(null);
            }
            if (_censor.Check(msg)) {
                return Result.Err(new ActionErr(ActionErrType.BannedWord));
            }

            var state = Player(player);
            var volume = state.EnforceMessageVolume(now);
            if (state.MutedUntil.HasValue) {
                return Result.Err(new ActionErr(ActionErrType.SpamMuted, state.MutedUntil.Value - now));
            }
            if (volume > 0.75f) return Result.Ok(ActionNote.SpamWarn);
            return Result.Ok(null);
        }
    }

    public readonly struct Result {
        public bool IsOk { get; }
        public ActionNote? Note { get; }
        public ActionErr? Error { get; }
        private Result(bool ok, ActionNote? note, ActionErr? err) {
            IsOk = ok; Note = note; Error = err;
        }
        public static Result Ok(ActionNote? note) => new Result(true, note, null);
        public static Result Err(ActionErr err) => new Result(false, null, err);
    }

    internal class PlayerState {
        public DateTime? LastMsgTime { get; private set; }
        public float ChatVolume { get; private set; }
        public DateTime? MutedUntil { get; private set; }

        private const float CHAT_VOLUME_PERIOD = 30f;
        private const float MAX_AVG_MSG_PER_SECOND = 1f / 5f;
        private static readonly TimeSpan SPAM_MUTE_PERIOD = TimeSpan.FromSeconds(180);

        public float EnforceMessageVolume(DateTime now) {
            if (MutedUntil.HasValue && MutedUntil.Value <= now) MutedUntil = null;
            if (LastMsgTime.HasValue) {
                var timeSince = (float)(now - LastMsgTime.Value).TotalSeconds;
                var timeProp = Math.Min(timeSince / CHAT_VOLUME_PERIOD, 1f);
                ChatVolume = ChatVolume * (1f - timeProp) + (1f / timeSince) * timeProp;
            } else {
                ChatVolume = 0f;
            }
            LastMsgTime = now;
            float min = 1f / CHAT_VOLUME_PERIOD;
            float max = MAX_AVG_MSG_PER_SECOND;
            float vol = Math.Max(0f, (ChatVolume - min) / (max - min));
            if (vol > 1f && !MutedUntil.HasValue) {
                MutedUntil = now + SPAM_MUTE_PERIOD;
            }
            return vol;
        }
    }
}
