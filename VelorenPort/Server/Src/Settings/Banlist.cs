using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using VelorenPort.CoreEngine;

namespace VelorenPort.Server.Settings {
    /// <summary>
    /// Simple ban list storage mirroring the Rust implementation in a minimal
    /// form. Entries are loaded from and saved to disk as JSON.
    /// </summary>
    public class Banlist {
        public class BanInfo {
            public string Reason { get; set; } = string.Empty;
            public long? Until { get; set; }
            public AdminRole PerformedByRole { get; set; } = AdminRole.Admin;
        }

        public record Ban(BanInfo Info, DateTime? EndDate) {
            public bool IsExpired(DateTime now) => EndDate.HasValue && EndDate.Value <= now;
            public AdminRole PerformedByRole() => Info.PerformedByRole;
            public BanInfo GetInfo() => Info;
        }

        public abstract record BanAction {
            public sealed record Unban(BanInfo Info) : BanAction;
            public sealed record Apply(Ban Data) : BanAction {
                public Ban Data { get; } = Data;
            }
            public Ban? AsBan() => this is Apply a ? a.Data : null;
        }

        public record BanRecord(string UsernameWhenPerformed, BanAction Action, DateTime Date) {
            public bool IsExpired(DateTime now) => Action switch {
                BanAction.Apply b => b.Data.IsExpired(now),
                _ => true
            };
        }

        public record BanEntry(BanRecord Current);

        private readonly Dictionary<Guid, BanEntry> _uuidBans = new();
        private readonly Dictionary<LoginProvider.NormalizedIpAddr, BanEntry> _ipBans = new();

        public IReadOnlyDictionary<Guid, BanEntry> UuidBans() => _uuidBans;
        public IReadOnlyDictionary<LoginProvider.NormalizedIpAddr, BanEntry> IpBans() => _ipBans;

        public void BanUuid(Guid uuid, string username, BanInfo info, DateTime? endDate) {
            _uuidBans[uuid] = new BanEntry(new BanRecord(username,
                new BanAction.Apply(new Ban(info, endDate)), DateTime.UtcNow));
        }

        public void UnbanUuid(Guid uuid, string username, BanInfo info) {
            _uuidBans[uuid] = new BanEntry(new BanRecord(username,
                new BanAction.Unban(info), DateTime.UtcNow));
        }

        public void BanIp(IPAddress ip, string username, BanInfo info, DateTime? endDate) {
            var key = new LoginProvider.NormalizedIpAddr(ip);
            _ipBans[key] = new BanEntry(new BanRecord(username,
                new BanAction.Apply(new Ban(info, endDate)), DateTime.UtcNow));
        }

        public void UnbanIp(IPAddress ip, string username, BanInfo info) {
            var key = new LoginProvider.NormalizedIpAddr(ip);
            _ipBans[key] = new BanEntry(new BanRecord(username,
                new BanAction.Unban(info), DateTime.UtcNow));
        }

        public bool TryGetActiveBan(Guid uuid, IPAddress? ip, DateTime now, out Ban? ban) {
            ban = null;
            if (_uuidBans.TryGetValue(uuid, out var be))
                ban = be.Current.Action.AsBan();
            if (ban == null && ip != null && _ipBans.TryGetValue(new LoginProvider.NormalizedIpAddr(ip), out var ibe))
                ban = ibe.Current.Action.AsBan();
            if (ban != null && LoginProvider.BanApplies(ban, null, now))
                return true;
            ban = null;
            return false;
        }

        public static Banlist Load(string path) {
            if (!File.Exists(path))
                return new Banlist();
            try {
                var json = File.ReadAllText(path);
                var data = JsonSerializer.Deserialize<BanFile>(json);
                var list = new Banlist();
                if (data != null) {
                    foreach (var b in data.UuidBans)
                        list._uuidBans[b.Uuid] = new BanEntry(b.Record);
                    foreach (var b in data.IpBans)
                        list._ipBans[new LoginProvider.NormalizedIpAddr(IPAddress.Parse(b.Ip))] = new BanEntry(b.Record);
                }
                return list;
            } catch {
                return new Banlist();
            }
        }

        public void Save(string path) {
            var data = new BanFile {
                UuidBans = new List<BanFileUuidEntry>(),
                IpBans = new List<BanFileIpEntry>()
            };
            foreach (var (uuid, entry) in _uuidBans)
                data.UuidBans.Add(new BanFileUuidEntry { Uuid = uuid, Record = entry.Current });
            foreach (var (ip, entry) in _ipBans)
                data.IpBans.Add(new BanFileIpEntry { Ip = ip.Address.ToString(), Record = entry.Current });
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }

        private class BanFile {
            public List<BanFileUuidEntry> UuidBans { get; set; } = new();
            public List<BanFileIpEntry> IpBans { get; set; } = new();
        }

        private class BanFileUuidEntry {
            public Guid Uuid { get; set; }
            public BanRecord Record { get; set; } = null!;
        }

        private class BanFileIpEntry {
            public string Ip { get; set; } = string.Empty;
            public BanRecord Record { get; set; } = null!;
        }
    }
}
