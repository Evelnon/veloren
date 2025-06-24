using System;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Represents the period of the day based on seconds since midnight.
    /// Matches the logic of DayPeriod in the original Rust code.
    /// </summary>
    public enum DayPeriod {
        Night,
        Morning,
        Noon,
        Evening,
    }

    public static class DayPeriodUtil {
        public static DayPeriod FromTimeOfDay(double seconds) {
            double tod = seconds % (60 * 60 * 24);
            if (tod < 0) tod += 60 * 60 * 24;
            if (tod < 60 * 60 * 6) return DayPeriod.Night;
            if (tod < 60 * 60 * 11) return DayPeriod.Morning;
            if (tod < 60 * 60 * 16) return DayPeriod.Noon;
            if (tod < 60 * 60 * 19) return DayPeriod.Evening;
            return DayPeriod.Night;
        }

        public static bool IsDark(this DayPeriod period) => period == DayPeriod.Night;
        public static bool IsLight(this DayPeriod period) => !period.IsDark();
    }
}
