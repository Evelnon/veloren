using System;

namespace VelorenPort.Network {
    /// <summary>
    /// Helper utilities used across the network subsystem.
    /// </summary>
    public static class Util {
        public static byte[] HexStringToBytes(string hex) {
            if (hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                hex = hex[2..];
            int length = hex.Length / 2;
            byte[] bytes = new byte[length];
            for (int i = 0; i < length; i++) {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return bytes;
        }

        public static string BytesToHexString(ReadOnlySpan<byte> bytes) {
            return BitConverter.ToString(bytes.ToArray()).Replace("-", "");
        }
    }
}
