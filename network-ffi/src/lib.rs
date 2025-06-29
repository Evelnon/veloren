use std::ffi::CStr;
use std::net::UdpSocket;
use std::os::raw::{c_char, c_int, c_uchar};

/// Envia un datagrama UDP a la direccion especificada.
/// Devuelve el numero de bytes enviados o -1 en caso de error.
#[unsafe(no_mangle)]
pub unsafe extern "C" fn vp_send_udp(addr: *const c_char, data: *const c_uchar, len: usize) -> c_int {
    if addr.is_null() || data.is_null() {
        return -1;
    }
    // Convertir la direccion C a &str
    let addr_cstr = CStr::from_ptr(addr);
    let addr_str = match addr_cstr.to_str() {
        Ok(s) => s,
        Err(_) => return -1,
    };
    let socket = match UdpSocket::bind("0.0.0.0:0") {
        Ok(s) => s,
        Err(_) => return -1,
    };
    let slice = std::slice::from_raw_parts(data, len);
    match socket.send_to(slice, addr_str) {
        Ok(sent) => sent as c_int,
        Err(_) => -1,
    }
}

/// Devuelve la version de la capa FFI.
#[unsafe(no_mangle)]
pub unsafe extern "C" fn vp_network_ffi_version() -> c_int {
    1
}
