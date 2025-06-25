using System.IO;

namespace VelorenPort.Server {
    /// <summary>
    /// Indicates where maps, saves and configuration folders are stored.
    /// Simple container around a <see cref="DirectoryInfo"/>. Mirrors
    /// <c>DataDir</c> from <c>server/src/data_dir.rs</c>.
    /// </summary>
    public class DataDir {
        /// <summary>
        /// Default directory name used by the Rust server to store data.
        /// </summary>
        public const string DefaultDataDirName = "server";

        /// <summary>
        /// Path to the data directory on disk.
        /// </summary>
        public DirectoryInfo Path { get; }

        public DataDir(DirectoryInfo path) {
            Path = path ?? throw new System.ArgumentNullException(nameof(path));
        }

        public static implicit operator DirectoryInfo(DataDir d) => d.Path;

        public override string ToString() => Path.FullName;
    }
}
