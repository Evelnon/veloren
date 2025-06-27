using VelorenPort.Network;

namespace VelorenPort.Server {
    /// <summary>
    /// Serialized message with associated stream id. Mirrors `PreparedMsg` in
    /// `server/src/client.rs`.
    /// </summary>
    public sealed class PreparedMsg {
        public byte StreamId { get; }
        public Message Message { get; }

        public PreparedMsg(byte streamId, Message message) {
            StreamId = streamId;
            Message = message;
        }

        public static PreparedMsg Create(byte streamId, object payload, StreamParams streamParams) {
            var msg = Message.Serialize(payload, streamParams);
            return new PreparedMsg(streamId, msg);
        }
    }
}
