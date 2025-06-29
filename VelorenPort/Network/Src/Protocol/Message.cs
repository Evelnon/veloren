using System;
using System.Collections.Generic;

namespace VelorenPort.Network.Protocol {
    /// <summary>
    /// Container for protocol payloads exchanged over streams. Stores the
    /// message id and stream id so frames map one-to-one with the Rust
    /// OTMessage/ITMessage structures.
    /// </summary>
    public class Message {
        /// <summary>Maximum payload size of an individual data frame.</summary>
        public const int FrameDataSize = 1400;

        public byte[] Data { get; }
        public ulong Mid { get; }
        public Sid Sid { get; }

        private int _offset;
        private bool _sentHeader;

        public Message(byte[] data, ulong mid, Sid sid) {
            Data = data;
            Mid = mid;
            Sid = sid;
        }

        /// <summary>
        /// Enumerates protocol frames representing this message.
        /// </summary>
        public IEnumerable<OTFrame> Serialize() {
            if (!_sentHeader) {
                _sentHeader = true;
                yield return new OTFrame.DataHeader(Mid, Sid, (ulong)Data.Length);
            }

            while (_offset < Data.Length) {
                int toSend = Math.Min(FrameDataSize, Data.Length - _offset);
                var chunk = new byte[toSend];
                Array.Copy(Data, _offset, chunk, 0, toSend);
                _offset += toSend;
                yield return new OTFrame.Data(Mid, chunk);
            }
        }

        /// <summary>
        /// Create a complete message from a header and the following data frames.
        /// </summary>
        public static Message Deserialize(ITFrame.DataHeader header, IEnumerable<ITFrame.Data> chunks) {
            var buffer = new byte[header.Length];
            int offset = 0;
            foreach (var c in chunks) {
                int copy = Math.Min(c.Payload.Length, buffer.Length - offset);
                Array.Copy(c.Payload, 0, buffer, offset, copy);
                offset += copy;
                if (offset >= buffer.Length) break;
            }
            return new Message(buffer, header.Mid, header.Sid);
        }
    }
}
