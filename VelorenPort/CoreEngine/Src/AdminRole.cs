namespace VelorenPort.CoreEngine;

/// <summary>
/// Administrative role assigned to a player. Mirrors `AdminRole` from
/// `common/src/comp/admin.rs`.
/// </summary>
public enum AdminRole {
    Moderator = 0,
    Admin = 1,
}

/// <summary>
/// Helper utilities to convert to and from strings just like the Rust
/// implementation which provides <c>FromStr</c> and <c>ToString</c>.
/// </summary>
public static class AdminRoleHelper {
    /// <summary>
    /// Parse a role from string. Accepts "mod", "moderator" or "admin".
    /// Returns <c>false</c> if the value could not be parsed.
    /// </summary>
    public static bool TryParse(string? value, out AdminRole role) {
        role = default;
        if (string.IsNullOrWhiteSpace(value)) {
            return false;
        }
        value = value.Trim().ToLowerInvariant();
        switch (value) {
            case "mod":
            case "moderator":
                role = AdminRole.Moderator;
                return true;
            case "admin":
                role = AdminRole.Admin;
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// Convert an <see cref="AdminRole"/> into the canonical string used in
    /// configuration files. Mirrors <c>ToString</c> from Rust.
    /// </summary>
    public static string ToRoleString(this AdminRole role) => role switch {
        AdminRole.Moderator => "moderator",
        AdminRole.Admin => "admin",
        _ => role.ToString().ToLowerInvariant(),
    };
}

/// <summary>
/// Helper component storing a player's administrative role.
/// </summary>
public struct Admin {
    public AdminRole Role { get; set; }
    public Admin(AdminRole role) { Role = role; }
}
