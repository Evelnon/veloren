namespace Network.Tests;

internal static class QuicFeatureSupport
{
    public static bool SupportsZeroRtt =>
        System.Environment.GetEnvironmentVariable("RUST_SERVER_SUPPORTS_0RTT") == "1";

    public static bool SupportsMigration =>
        System.Environment.GetEnvironmentVariable("RUST_SERVER_SUPPORTS_MIGRATION") == "1";
}
