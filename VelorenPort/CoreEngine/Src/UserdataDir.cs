using System;
using System.IO;
using System.Runtime.InteropServices;

#nullable enable

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Provides the same logic as <c>userdata_dir.rs</c> from the Rust project.
    /// Determines the directory used to store persistent user data.
    /// </summary>
    public static class UserdataDir {
        private const string UserDataEnv = "VELOREN_USERDATA";
        private const string UserDataStrategyEnv = "VELOREN_USERDATA_STRATEGY";

        public static DirectoryInfo GetUserdataDir(bool workspace, string? strategy, string manifestDir) {
            // 1. Environment variable override
            string? envPath = Environment.GetEnvironmentVariable(UserDataEnv);
            if (!string.IsNullOrEmpty(envPath)) {
                return new DirectoryInfo(envPath);
            }

            // 2. Strategy environment variable
            if (!string.IsNullOrEmpty(strategy)) {
                if (string.Equals(strategy, "system", StringComparison.OrdinalIgnoreCase)) {
                    string basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    return new DirectoryInfo(Path.Combine(basePath, "veloren", "veloren", "userdata"));
                }
                if (string.Equals(strategy, "executable", StringComparison.OrdinalIgnoreCase)) {
                    return new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "userdata"));
                }

                Console.Error.WriteLine($"Compiled with an invalid VELOREN_USERDATA_STRATEGY: \"{strategy}\". " +
                    "Valid values are unset, \"system\", and \"executable\". Falling back to unset case.");
            }

            // 3. Relative to the manifest directory
            var path = new DirectoryInfo(manifestDir);
            if (workspace && path.Parent != null)
                path = path.Parent;
            var exePath = new DirectoryInfo(AppContext.BaseDirectory);
            if (path.Exists && exePath.FullName.StartsWith(path.FullName, StringComparison.Ordinal)) {
                return new DirectoryInfo(Path.Combine(path.FullName, "userdata"));
            }

            var fallback = new DirectoryInfo(Path.Combine(exePath.FullName, "userdata"));
            Console.Error.WriteLine(
                $"This binary is outside the project folder where it was compiled ({path.FullName}) and was not compiled with VELOREN_USERDATA_STRATEGY set to \"system\" or \"executable\". " +
                $"Falling back to the \"executable\" strategy (the userdata folder will be placed in the same folder as the executable: {fallback.FullName}).\n" +
                $"NOTE: You can manually select a userdata folder by setting the environment variable {UserDataEnv} to the desired directory before running.");
            return fallback;
        }

        public static DirectoryInfo Workspace() {
            string manifest = AppContext.BaseDirectory;
            string? strategy = Environment.GetEnvironmentVariable(UserDataStrategyEnv);
            return GetUserdataDir(true, strategy, manifest);
        }

        public static DirectoryInfo NoWorkspace() {
            string manifest = AppContext.BaseDirectory;
            string? strategy = Environment.GetEnvironmentVariable(UserDataStrategyEnv);
            return GetUserdataDir(false, strategy, manifest);
        }
    }
}
