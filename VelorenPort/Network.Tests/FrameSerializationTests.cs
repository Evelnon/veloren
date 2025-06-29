using System;
using Xunit;
using VelorenPort.Network;
using VelorenPort.Network.Protocol;

namespace Network.Tests;

public class FrameSerializationTests
{
    [Fact]
    public void InitFrameRoundtrip()
    {
        var frames = new InitFrame[]
        {
            new InitFrame.Handshake(Network.Handshake.MagicNumber, Network.Handshake.SupportedVersion),
            new InitFrame.Init(new Pid(Guid.NewGuid()), Guid.Empty),
            new InitFrame.Raw(new byte[]{1,2,3})
        };

        foreach(var f in frames)
        {
            var bytes = FrameCodec.Serialize(f);
            Assert.True(FrameCodec.TryDeserializeInit(bytes, out var parsed, out var consumed));
            Assert.Equal(bytes.Length, consumed);
            Assert.Equal(f, parsed);
        }
    }

    [Fact]
    public void OTFrameRoundtrip()
    {
        var frames = new OTFrame[]
        {
            new OTFrame.Shutdown(),
            new OTFrame.OpenStream(new Sid(42), 3, Promises.GuaranteedDelivery, 10),
            new OTFrame.CloseStream(new Sid(7)),
            new OTFrame.DataHeader(1, new Sid(5), 8),
            new OTFrame.Data(2, new byte[]{10,20})
        };

        foreach(var f in frames)
        {
            var bytes = FrameCodec.Serialize(f);
            Assert.True(FrameCodec.TryDeserializeIT(bytes, out var parsed, out var consumed));
            Assert.Equal(bytes.Length, consumed);
            Assert.Equal(ToIt(f), parsed);
        }
    }

    private static ITFrame ToIt(OTFrame f) => f switch
    {
        OTFrame.Shutdown => new ITFrame.Shutdown(),
        OTFrame.OpenStream(var sid, var prio, var promises, var bw) => new ITFrame.OpenStream(sid, prio, promises, bw),
        OTFrame.CloseStream(var sid) => new ITFrame.CloseStream(sid),
        OTFrame.DataHeader(var mid, var sid, var len) => new ITFrame.DataHeader(mid, sid, len),
        OTFrame.Data(var mid, var p) => new ITFrame.Data(mid, p),
        _ => throw new InvalidOperationException()
    };
}
