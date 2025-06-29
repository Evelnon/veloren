using System;

namespace VelorenPort.Network {
    /// <summary>
    /// Simple credential container used during participant creation.
    /// </summary>
    public readonly struct Credentials {
        public string Value { get; }
        public Credentials(string value) { Value = value; }
        public bool IsValid => Guid.TryParse(Value, out _);
    }
}
