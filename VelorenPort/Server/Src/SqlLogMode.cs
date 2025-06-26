using System;

namespace VelorenPort.Server {
    /// <summary>
    /// Logging levels for SQL statements, matching <c>SqlLogMode</c> in
    /// <c>server/src/persistence/mod.rs</c>.
    /// </summary>
    public enum SqlLogMode {
        Disabled,
        Profile,
        Trace,
    }

    public static class SqlLogModeExtensions {
        public static bool TryParse(string? value, out SqlLogMode mode) {
            mode = SqlLogMode.Disabled;
            if (string.IsNullOrWhiteSpace(value)) return false;
            switch (value.Trim().ToLowerInvariant()) {
                case "disabled":
                    mode = SqlLogMode.Disabled;
                    return true;
                case "profile":
                    mode = SqlLogMode.Profile;
                    return true;
                case "trace":
                    mode = SqlLogMode.Trace;
                    return true;
                default:
                    return false;
            }
        }

        public static string ToModeString(this SqlLogMode mode) => mode switch {
            SqlLogMode.Disabled => "disabled",
            SqlLogMode.Profile => "profile",
            SqlLogMode.Trace => "trace",
            _ => mode.ToString().ToLowerInvariant(),
        };
    }
}
