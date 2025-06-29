using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace VelorenPort.Network {
    internal sealed class UdpHandshakeStream : Stream {
        private readonly UdpClient _client;
        private readonly IPEndPoint _remote;
        private byte[] _buffer = Array.Empty<byte>();
        private int _pos;

        public UdpHandshakeStream(UdpClient client, IPEndPoint remote, byte[]? first = null) {
            _client = client;
            _remote = remote;
            if (first != null && first.Length > 0) {
                _buffer = first;
                _pos = 0;
            }
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public override void Flush() { }
        public override Task FlushAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public override int Read(byte[] buffer, int offset, int count) =>
            ReadAsync(buffer.AsMemory(offset, count), CancellationToken.None).Result;

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
            await ReadAsync(buffer.AsMemory(offset, count), cancellationToken);

        public override async ValueTask<int> ReadAsync(Memory<byte> destination, CancellationToken cancellationToken = default) {
            if (_pos >= _buffer.Length) {
                UdpReceiveResult result;
                do {
                    result = await _client.ReceiveAsync(cancellationToken);
                } while (!result.RemoteEndPoint.Equals(_remote));
                _buffer = result.Buffer;
                _pos = 0;
            }
            int toCopy = Math.Min(destination.Length, _buffer.Length - _pos);
            _buffer.AsSpan(_pos, toCopy).CopyTo(destination.Span);
            _pos += toCopy;
            return toCopy;
        }

        public override void Write(byte[] buffer, int offset, int count) =>
            WriteAsync(buffer.AsMemory(offset, count), CancellationToken.None).GetAwaiter().GetResult();

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
            await WriteAsync(buffer.AsMemory(offset, count), cancellationToken);

        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> source, CancellationToken cancellationToken = default) {
            await _client.SendAsync(source.ToArray(), source.Length, _remote);
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
    }
}
