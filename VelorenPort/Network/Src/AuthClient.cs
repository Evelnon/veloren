using System;
using System.Threading.Tasks;

namespace VelorenPort.Network {
    /// <summary>
    /// Minimal stub of the authentication client used by <see cref="Server.LoginProvider"/>.
    /// </summary>
    public class AuthClient {
        public Task<Guid> Validate(AuthToken token) => Task.FromResult(Guid.Parse(token.Value));
        public Task<string> UuidToUsername(Guid uuid) => Task.FromResult(uuid.ToString());
        public Task<Guid> UsernameToUuid(string username) => Task.FromResult(DeriveUuid(username));

        public static Guid DeriveUuid(string username) {
            System.Numerics.BigInteger state = System.Numerics.BigInteger.Parse("144066263297769815596495629667062367629");
            foreach (byte b in System.Text.Encoding.UTF8.GetBytes(username)) {
                state ^= b;
                state = (state * System.Numerics.BigInteger.Parse("309485009821345068724781371")) & ((System.Numerics.BigInteger.One << 128) - 1);
            }
            var bytes = state.ToByteArray();
            Array.Resize(ref bytes, 16);
            return new Guid(bytes);
        }
    }

    public readonly struct AuthToken {
        public string Value { get; }
        public AuthToken(string value) { Value = value; }
        public static AuthToken FromString(string s) => new AuthToken(s);
    }

    public class AuthClientError : Exception {
        public AuthClientError(string message) : base(message) { }
    }
}
