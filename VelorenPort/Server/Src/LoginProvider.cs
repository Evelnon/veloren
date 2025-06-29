using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using VelorenPort.CoreEngine;
using VelorenPort.Network;
using Ban = VelorenPort.Server.Settings.Banlist.Ban;
using BanInfo = VelorenPort.Server.Settings.Banlist.BanInfo;
using VelorenPort.Server.Settings;

namespace VelorenPort.Server {
    public static class LoginUtils {
        public static bool BanApplies(Ban ban, AdminList.AdminRecord? admin, DateTime now) {
            bool ExceedsBanRole(AdminList.AdminRecord a) => a.Role >= ban.PerformedByRole();
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

        private readonly AdminList _admins;
        private readonly Banlist _banlist;
        private readonly Whitelist _whitelist;
        private readonly string _adminsPath;
        private readonly string _banlistPath;
        private readonly string _whitelistPath;
        public LoginProvider(string? authAddr, string? dataDir = null) {
            if (authAddr != null) {
                _authServer = new AuthClient();
            }
            var dir = dataDir ?? DataDir.DefaultDataDirName;
            _adminsPath = Path.Combine(dir, "admins.json");
            _banlistPath = Path.Combine(dir, "banlist.json");
            _whitelistPath = Path.Combine(dir, "whitelist.json");
            _admins = AdminList.Load(_adminsPath);
            _banlist = Banlist.Load(_banlistPath);
            _whitelist = Whitelist.Load(_whitelistPath);
        }

        public AdminList Admins => _admins;
        public Banlist Banlist => _banlist;
        public Whitelist Whitelist => _whitelist;

        public void Save() {
            _admins.Save(_adminsPath);
            _banlist.Save(_banlistPath);
            _whitelist.Save(_whitelistPath);
        }

        public PendingLogin Verify(string usernameOrToken) {
            var pending = new PendingLogin();
            if (_authServer != null) {
                var token = AuthToken.FromString(usernameOrToken);
                Task.Run(async () => {
                    try {
                        var uuid = await _authServer.Validate(token);
                        var username = await _authServer.UuidToUsername(uuid);
                        if (PreCheck(username, uuid, out var err))
                            pending.Set(Result<(string, Guid), RegisterError>.Err(err!));
                        else
                            pending.Set(Result<(string, Guid), RegisterError>.Ok((username, uuid)));
                    } catch (Exception e) {
                        pending.Set(Result<(string, Guid), RegisterError>.Err(new RegisterError.AuthError(e.Message)));
                    }
                });
            } else {
                var uuid = AuthClient.DeriveUuid(usernameOrToken);
                if (PreCheck(usernameOrToken, uuid, out var err))
                    pending.Set(Result<(string, Guid), RegisterError>.Err(err!));
                else
                    pending.Set(Result<(string, Guid), RegisterError>.Ok((usernameOrToken, uuid)));
            }
            return pending;
        }

        private bool PreCheck(string username, Guid uuid, out RegisterError? err) {
            err = null;
            if (_banlist.TryGetActiveBan(uuid, null, DateTime.UtcNow, out var ban)) {
                err = new RegisterError.Banned(ban!.GetInfo());
                return true;
            }
            if (!_admins.Admins.ContainsKey(uuid) && _whitelist.Entries.Count > 0 && !_whitelist.Contains(uuid)) {
                err = new RegisterError.NotOnWhitelist();
                return true;
            }
            return false;
        }

        public Result<R, RegisterError>? Login<R>(PendingLogin pending, Client client,
            Func<string, Guid, (bool exceeded, R res)> playerCountExceeded) {
            var admins = new Dictionary<Guid, AdminList.AdminRecord>(_admins.Admins);
            var wl = new Dictionary<Guid, Whitelist.WhitelistRecord>(_whitelist.Entries);
            return Login(pending, client, admins, wl, _banlist, playerCountExceeded);
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
            Dictionary<Guid, AdminList.AdminRecord> admins,
            Dictionary<Guid, Whitelist.WhitelistRecord> whitelist,
            Banlist banlist,
            Func<string, Guid, (bool exceeded, R res)> playerCountExceeded) {
            if (!pending.TryRecv(out var res)) return null;
            if (!res.IsOk) return Result<R, RegisterError>.Err(res.Err);
            var (username, uuid) = res.Ok;
            var now = DateTime.UtcNow;
            var ip = client.ConnectedFromAddr.SocketAddr?.Address;
            AdminList.AdminRecord? admin = admins.TryGetValue(uuid, out var ar) ? ar : null;
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
