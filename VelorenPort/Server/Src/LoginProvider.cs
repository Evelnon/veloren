using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using VelorenPort.CoreEngine;
using VelorenPort.Network;

namespace VelorenPort.Server {
    public static class LoginUtils {
        public static bool BanApplies(Ban ban, AdminRecord? admin, DateTime now) {
            bool ExceedsBanRole(AdminRecord a) => a.Role >= ban.PerformedByRole();
            return !ban.IsExpired(now) && !(admin != null && ExceedsBanRole(admin.Value));
        }
    }

    public readonly struct NormalizedIpAddr : IEquatable<NormalizedIpAddr> {
        public IPAddress Address { get; }
        public NormalizedIpAddr(IPAddress addr) {
            if (addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6) {
                var bytes = addr.GetAddressBytes();
                for (int i = 8; i < 16; i++) bytes[i] = 0;
                Address = new IPAddress(bytes);
            } else {
                Address = addr;
            }
        }
        public bool Equals(NormalizedIpAddr other) => Address.Equals(other.Address);
        public override int GetHashCode() => Address.GetHashCode();
    }

    public record AdminRecord(Guid Uuid, AdminRole Role);
    public record WhitelistRecord(Guid Uuid);

    public record BanInfo(string Reason, long? Until);

    public record Ban(BanInfo Info, DateTime? EndDate) {
        public bool IsExpired(DateTime now) => EndDate.HasValue && EndDate.Value <= now;
        public AdminRole PerformedByRole() => AdminRole.Admin;
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

    public class Banlist {
        private readonly Dictionary<Guid, BanEntry> _uuidBans = new();
        private readonly Dictionary<NormalizedIpAddr, BanEntry> _ipBans = new();
        public IReadOnlyDictionary<Guid, BanEntry> UuidBans() => _uuidBans;
        public IReadOnlyDictionary<NormalizedIpAddr, BanEntry> IpBans() => _ipBans;
    }

    public class PendingLogin {
        private readonly TaskCompletionSource<Result<(string, Guid), RegisterError>> _tcs = new();
        internal void Set(Result<(string, Guid), RegisterError> res) => _tcs.TrySetResult(res);
        internal bool TryRecv(out Result<(string, Guid), RegisterError> res) {
            if (_tcs.Task.IsCompleted) { res = _tcs.Task.Result; return true; }
            res = default!; return false;
        }
        public static PendingLogin NewSuccess(string username, Guid uuid) {
            var p = new PendingLogin();
            p.Set(Result<(string, Guid), RegisterError>.Ok((username, uuid)));
            return p;
        }
    }

    public class LoginProvider {
        private readonly AuthClient? _authServer;

        public LoginProvider(string? authAddr) {
            if (authAddr != null) {
                _authServer = new AuthClient();
            }
        }

        public PendingLogin Verify(string usernameOrToken) {
            var pending = new PendingLogin();
            if (_authServer != null) {
                var token = AuthToken.FromString(usernameOrToken);
                Task.Run(async () => {
                    try {
                        var uuid = await _authServer.Validate(token);
                        var username = await _authServer.UuidToUsername(uuid);
                        pending.Set(Result<(string, Guid), RegisterError>.Ok((username, uuid)));
                    } catch (Exception e) {
                        pending.Set(Result<(string, Guid), RegisterError>.Err(new RegisterError.AuthError(e.Message)));
                    }
                });
            } else {
                var uuid = AuthClient.DeriveUuid(usernameOrToken);
                pending.Set(Result<(string, Guid), RegisterError>.Ok((usernameOrToken, uuid)));
            }
            return pending;
        }

        public static Guid DeriveUuid(string username) => AuthClient.DeriveUuid(username);

        public static Guid DeriveSingleplayerUuid() => DeriveUuid("singleplayer");

        public Result<Guid, AuthClientError> UsernameToUuid(string username) {
            if (_authServer != null) {
                try {
                    var uuid = _authServer.UsernameToUuid(username)
                        .GetAwaiter().GetResult();
                    return Result<Guid, AuthClientError>.Ok(uuid);
                } catch (Exception e) {
                    return Result<Guid, AuthClientError>.Err(new AuthClientError(e.Message));
                }
            }
            return Result<Guid, AuthClientError>.Ok(DeriveUuid(username));
        }

        public async Task<Result<Guid, AuthClientError>> UsernameToUuidAsync(string username) {
            if (_authServer != null) {
                try {
                    var uuid = await _authServer.UsernameToUuid(username);
                    return Result<Guid, AuthClientError>.Ok(uuid);
                } catch (Exception e) {
                    return Result<Guid, AuthClientError>.Err(new AuthClientError(e.Message));
                }
            }
            return Result<Guid, AuthClientError>.Ok(DeriveUuid(username));
        }

        public Result<string, AuthClientError> UuidToUsername(Guid uuid, string fallbackAlias) {
            if (_authServer != null) {
                try {
                    var name = _authServer.UuidToUsername(uuid)
                        .GetAwaiter().GetResult();
                    return Result<string, AuthClientError>.Ok(name);
                } catch (Exception e) {
                    return Result<string, AuthClientError>.Err(new AuthClientError(e.Message));
                }
            }
            return Result<string, AuthClientError>.Ok(fallbackAlias);
        }

        public async Task<Result<string, AuthClientError>> UuidToUsernameAsync(Guid uuid, string fallbackAlias) {
            if (_authServer != null) {
                try {
                    var name = await _authServer.UuidToUsername(uuid);
                    return Result<string, AuthClientError>.Ok(name);
                } catch (Exception e) {
                    return Result<string, AuthClientError>.Err(new AuthClientError(e.Message));
                }
            }
            return Result<string, AuthClientError>.Ok(fallbackAlias);
        }

        public static Result<R, RegisterError>? Login<R>(PendingLogin pending, Client client,
            Dictionary<Guid, AdminRecord> admins,
            Dictionary<Guid, WhitelistRecord> whitelist,
            Banlist banlist,
            Func<string, Guid, (bool exceeded, R res)> playerCountExceeded) {
            if (!pending.TryRecv(out var res)) return null;
            if (!res.IsOk) return Result<R, RegisterError>.Err(res.Err);
            var (username, uuid) = res.Ok;
            var now = DateTime.UtcNow;
            var ip = client.ConnectedFromAddr.SocketAddr?.Address;
            AdminRecord? admin = admins.TryGetValue(uuid, out var ar) ? ar : null;
            Ban? ban = null;
            if (banlist.UuidBans().TryGetValue(uuid, out var be)) ban = be.Current.Action.AsBan();
            if (ban == null && ip != null && banlist.IpBans().TryGetValue(new NormalizedIpAddr(ip), out var ibe))
                ban = ibe.Current.Action.AsBan();
            if (ban != null && LoginUtils.BanApplies(ban, admin, now))
                return Result<R, RegisterError>.Err(new RegisterError.Banned(ban.Info()));
            if (admin == null && whitelist.Count > 0 && !whitelist.ContainsKey(uuid))
                return Result<R, RegisterError>.Err(new RegisterError.NotOnWhitelist());
            var (exceeded, result) = playerCountExceeded(username, uuid);
            if (admin == null && exceeded)
                return Result<R, RegisterError>.Err(new RegisterError.TooManyPlayers());
            return Result<R, RegisterError>.Ok(result);
        }
    }
}
