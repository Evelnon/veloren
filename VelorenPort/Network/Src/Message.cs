using System;
using System.IO;
using System.IO.Compression;
using System.Text.Json;

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
            // Serialize the payload using System.Text.Json and prefix it with a
            // protobuf-style length field. This avoids BinaryFormatter
            // dependency and matches the Rust implementation more closely.
            using var ms = new MemoryStream();
            var json = JsonSerializer.SerializeToUtf8Bytes(payload);
            WriteVarInt(json.Length, ms);
            ms.Write(json, 0, json.Length);
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
            // Decompress if required then read the varint length and decode the
            // JSON payload.
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
            int len = ReadVarInt(ms);
            var jsonBytes = new byte[len];
            _ = ms.Read(jsonBytes, 0, len);
            return JsonSerializer.Deserialize<T>(jsonBytes)!;
        }

        private static void WriteVarInt(int value, Stream stream)
        {
            uint v = (uint)value;
            while (v >= 0x80)
            {
                stream.WriteByte((byte)(v | 0x80));
                v >>= 7;
            }
            stream.WriteByte((byte)v);
        }

        private static int ReadVarInt(Stream stream)
        {
            int value = 0;
            int shift = 0;
            int b;
            do
            {
                b = stream.ReadByte();
                if (b == -1) throw new EndOfStreamException();
                value |= (b & 0x7F) << shift;
                shift += 7;
            } while ((b & 0x80) != 0);
            return value;
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
