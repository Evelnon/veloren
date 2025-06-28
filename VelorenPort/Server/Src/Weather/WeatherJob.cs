namespace VelorenPort.Server.Weather {
    /// <summary>
    /// Represents a queued weather update. In the original server this would
    /// update cloud cover, rain and other effects. Here we simply keep a time
    /// until the next weather tick.
    /// </summary>
    public class WeatherJob {
        public float NextUpdate { get; set; }
    }
}
