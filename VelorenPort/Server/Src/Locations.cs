using System;
using System.Collections.Generic;
using VelorenPort.NativeMath;

namespace VelorenPort.Server {
    /// <summary>
    /// Error codes returned by <see cref="Locations"/> methods.
    /// Mirrors <c>LocationError</c> from server/src/location.rs.
    /// </summary>
    public abstract record LocationError {
        public sealed record InvalidName(string Name) : LocationError;
        public sealed record DuplicateName(string Name) : LocationError;
        public sealed record DoesNotExist(string Name) : LocationError;
    }

    /// <summary>
    /// Moderator-defined teleport locations. They are stored only in memory and
    /// do not persist between sessions yet.
    /// </summary>
    public class Locations {
        private readonly Dictionary<string, float3> _locations = new();

        /// <summary>
        /// Insert a new location. Names may only contain lowercase ASCII
        /// characters or underscores.
        /// </summary>
        public LocationError? Insert(string name, float3 pos) {
            if (!IsValidName(name))
                return new LocationError.InvalidName(name);
            if (!_locations.TryAdd(name, pos))
                return new LocationError.DuplicateName(name);
            return null;
        }

        /// <summary>Retrieve a location by name.</summary>
        public bool TryGet(string name, out float3 pos) {
            return _locations.TryGetValue(name, out pos);
        }

        /// <summary>Enumerate all location names.</summary>
        public IEnumerable<string> Names => _locations.Keys;

        /// <summary>Remove a location.</summary>
        public LocationError? Remove(string name) {
            if (_locations.Remove(name))
                return null;
            return new LocationError.DoesNotExist(name);
        }

        private static bool IsValidName(string name) {
            foreach (char c in name) {
                if (!(char.IsAscii(c) && (char.IsLower(c) || c == '_')))
                    return false;
            }
            return true;
        }
    }
}
