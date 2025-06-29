using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;

namespace VelorenPort.Network {
    /// <summary>
    /// Serializable message container similar to the Rust implementation.
    /// </summary>
    [Serializable]
    public class Message {
        public byte[] Data { get; private set; }
        public bool Compressed { get; private set; }

        private Message(byte[] data, bool compressed) {
            Data = data;
            Compressed = compressed;
        }

        public static Message Serialize(object payload, StreamParams parameters) {
            using var ms = new MemoryStream();
            var formatter = new BinaryFormatter();
            formatter.Serialize(ms, payload);
            var bytes = ms.ToArray();

            if (parameters.Promises.HasFlag(Promises.Compressed)) {
                using var msOut = new MemoryStream();
                using (var gzip = new GZipStream(msOut, CompressionLevel.Fastest)) {
                    gzip.Write(bytes, 0, bytes.Length);
                }
                bytes = msOut.ToArray();
                return new Message(bytes, true);
            }

            return new Message(bytes, false);
        }

        public T Deserialize<T>() {
            byte[] data = Data;
            if (Compressed) {
                using var msIn = new MemoryStream(Data);
                using var msOut = new MemoryStream();
                using (var gzip = new GZipStream(msIn, CompressionMode.Decompress)) {
                    gzip.CopyTo(msOut);
                }
                data = msOut.ToArray();
            }
            using var ms = new MemoryStream(data);
            var formatter = new BinaryFormatter();
            return (T)formatter.Deserialize(ms);
        }

#if DEBUG
        /// <summary>
        /// Verifies that the message compression matches the provided stream
        /// parameters. Only compiled in debug builds.
        /// </summary>
        public void Verify(StreamParams parameters)
        {
            bool expectCompressed = parameters.Promises.HasFlag(Promises.Compressed);
            if (expectCompressed != Compressed)
            {
                throw new InvalidOperationException(
                    $"Message compression mismatch. Expected {(expectCompressed ? "compressed" : "raw")}");
            }
        }
#endif
    }
}
