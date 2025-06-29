using System;
using VelorenPort.Network;

namespace Network.Tests;

public class HandshakeVersionTests
{
    [Fact]
    public void TryParseAcceptsLegacyVersion()
    {
        var pid = Pid.NewPid();
        var secret = Guid.NewGuid();
        var headerSize = Handshake.MagicNumber.Length + Handshake.SupportedVersion.Length * 4;
        var buffer = new byte[headerSize + 32];
        Array.Copy(Handshake.MagicNumber, buffer, Handshake.MagicNumber.Length);
        Buffer.BlockCopy(BitConverter.GetBytes(0u), 0, buffer, Handshake.MagicNumber.Length + 0, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(5u), 0, buffer, Handshake.MagicNumber.Length + 4, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(0u), 0, buffer, Handshake.MagicNumber.Length + 8, 4);
        var pidBytes = pid.ToByteArray();
        Array.Copy(pidBytes, 0, buffer, headerSize, 16);
        var secBytes = secret.ToByteArray();
        Array.Copy(secBytes, 0, buffer, headerSize + 16, 16);
        Assert.True(Handshake.TryParse(buffer, out var rp, out var rs, out var feat, out var ver));
        Assert.Equal(pid, rp);
        Assert.Equal(secret, rs);
        Assert.Equal(HandshakeFeatures.None, feat);
        Assert.Equal(new uint[] {0u,5u,0u}, ver);
    }

    [Fact]
    public void TryParseRejectsFutureVersion()
    {
        var pid = Pid.NewPid();
        var secret = Guid.NewGuid();
        var headerSize = Handshake.MagicNumber.Length + Handshake.SupportedVersion.Length * 4;
        var buffer = new byte[headerSize + 32];
        Array.Copy(Handshake.MagicNumber, buffer, Handshake.MagicNumber.Length);
        Buffer.BlockCopy(BitConverter.GetBytes(0u), 0, buffer, Handshake.MagicNumber.Length + 0, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(9u), 0, buffer, Handshake.MagicNumber.Length + 4, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(0u), 0, buffer, Handshake.MagicNumber.Length + 8, 4);
        var pidBytes = pid.ToByteArray();
        Array.Copy(pidBytes, 0, buffer, headerSize, 16);
        var secBytes = secret.ToByteArray();
        Array.Copy(secBytes, 0, buffer, headerSize + 16, 16);
        Assert.False(Handshake.TryParse(buffer, out _, out _, out _, out _));
    }

    [Fact]
    public void TryParseRoundTripWithFeatures()
    {
        var pid = Pid.NewPid();
        var secret = Guid.NewGuid();
        var features = HandshakeFeatures.Compression | HandshakeFeatures.Encryption;
        var data = Handshake.GetBytes(pid, secret, features);
        Assert.True(Handshake.TryParse(data, out var rp, out var rs, out var feat, out var ver));
        Assert.Equal(pid, rp);
        Assert.Equal(secret, rs);
        Assert.Equal(features, feat);
        Assert.Equal(Handshake.SupportedVersion, ver);
    }
}
