using System;
using VelorenPort.CoreEngine;

namespace VelorenPort.Simulation {
    /// <summary>
    /// Core rtsim data resource. Implements the minimum set of fields
    /// required for event routing and ticking. Additional fields from the
    /// original Rust version will be introduced as their dependencies are
    /// ported.
    /// </summary>
    [Serializable]
    public class Data {
        public const uint CURRENT_VERSION = 9;

        /// <summary>
        /// The version number of the serialized data format.
        /// </summary>
        public uint Version { get; set; } = CURRENT_VERSION;

        public ulong Tick { get; set; }
        public TimeOfDay TimeOfDay { get; set; }

        public Npcs Npcs { get; } = new();
        public Sites Sites { get; } = new();

        /// <summary>
        /// If <c>true</c>, rtsim data will be ignored and regenerated on load.
        /// </summary>
        public bool ShouldPurge { get; set; }

        public enum ReadError
        {
            None,
            Load,
            VersionMismatch,
        }

        /// <summary>
        /// Attempt to load rtsim data from a stream. If the version does not
        /// match <see cref="CURRENT_VERSION"/>, the partially deserialized data
        /// is returned alongside a <see cref="ReadError.VersionMismatch"/> code.
        /// </summary>
        public static (Data? Data, ReadError Error) ReadFrom(System.IO.Stream stream)
        {
            try
            {
                var data = System.Text.Json.JsonSerializer.Deserialize<Data>(stream);
                if (data == null)
                    return (null, ReadError.Load);
                return data.Version == CURRENT_VERSION
                    ? (data, ReadError.None)
                    : (data, ReadError.VersionMismatch);
            }
            catch (System.Text.Json.JsonException)
            {
                return (null, ReadError.Load);
            }
        }

        /// <summary>
        /// Write the data to a stream using JSON serialization.
        /// </summary>
        public void WriteTo(System.IO.Stream stream)
        {
            System.Text.Json.JsonSerializer.Serialize(stream, this);
        }

        /// <summary>
        /// Add an NPC to the simulation, registering them with their home site i
        /// if provided.
        /// </summary>
        public NpcId SpawnNpc(Npc npc)
        {
            var id = Npcs.CreateNpc(npc);
            if (npc.Home is SiteId home)
            {
                Sites.Get(home)?.Population.Add(id);
            }
            return id;
        }
    }
}
