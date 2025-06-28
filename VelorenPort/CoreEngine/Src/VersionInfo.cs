using System;
using System.Globalization;

namespace VelorenPort.CoreEngine
{
    /// <summary>
    /// Provides Git version information similar to util/mod.rs constants.
    /// Values are resolved from environment variables each time accessed.
    /// </summary>
    public static class VersionInfo
    {
        public const string VelorenVersionStage = "Pre-Alpha";

        public static string GitVersion => Environment.GetEnvironmentVariable("VELOREN_GIT_VERSION") ?? string.Empty;
        public static string GitTag => Environment.GetEnvironmentVariable("VELOREN_GIT_TAG") ?? string.Empty;

        public static string GitHash
        {
            get
            {
                var ver = GitVersion;
                var idx = ver.IndexOf('/');
                return idx >= 0 ? ver[..idx] : ver;
            }
        }

        public static string GitDate
        {
            get
            {
                var ver = GitVersion;
                var idx = ver.IndexOf('/');
                if (idx < 0) return string.Empty;
                var dt = ver[(idx + 1)..];
                var parts = dt.Split('-');
                return parts.Length >= 3 ? string.Join("-", parts[0], parts[1], parts[2]) : string.Empty;
            }
        }

        public static string GitTime
        {
            get
            {
                var ver = GitVersion;
                var idx = ver.IndexOf('/');
                if (idx < 0) return string.Empty;
                var dt = ver[(idx + 1)..];
                var parts = dt.Split('-');
                return parts.Length >= 4 ? parts[3] : string.Empty;
            }
        }

        public static long GitDateTimestamp
        {
            get
            {
                var dtString = string.IsNullOrEmpty(GitDate) || string.IsNullOrEmpty(GitTime)
                    ? null
                    : $"{GitDate}-{GitTime}";
                if (dtString == null) return 0;
                if (DateTime.TryParseExact(dtString, "yyyy-MM-dd-HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dt))
                    return new DateTimeOffset(dt).ToUnixTimeSeconds();
                return 0;
            }
        }

        public static string DisplayVersion
        {
            get
            {
                var tag = GitTag;
                return string.IsNullOrEmpty(tag)
                    ? $"{VelorenVersionStage}-{GitDate}"
                    : $"{VelorenVersionStage}-{tag}";
            }
        }

        public static string DisplayVersionLong
        {
            get
            {
                var tag = GitTag;
                return string.IsNullOrEmpty(tag)
                    ? $"{DisplayVersion} ({GitHash})"
                    : $"{DisplayVersion} ({GitVersion})";
            }
        }
    }
}
