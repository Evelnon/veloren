using System.Runtime.InteropServices;

namespace VelorenPort.Network {
    /// <summary>
    /// Enlaces a funciones de networking implementadas en Rust mediante FFI.
    /// </summary>
    internal static class RustBindings {
        private const string Dll = "network_ffi";

        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "vp_send_udp")]
        internal static extern int SendUdp(string addr, byte[] data, ulong len);

        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl, EntryPoint = "vp_network_ffi_version")]
        internal static extern int Version();
    }
}
