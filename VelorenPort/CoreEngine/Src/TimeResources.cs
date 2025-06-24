using System;
using Unity.Mathematics;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Represents the time of day in seconds since midnight.
    /// Provides sun and moon direction helpers similar to the original Rust code.
    /// </summary>
    [Serializable]
    public struct TimeOfDay {
        public double Value;

        public TimeOfDay(double value) { Value = value; }

        private const double FULL_DAY = 24.0 * 3600.0;
        private const double TWO_PI = math.PI * 2.0;

        private float GetAngleRad() {
            const double TIME_FACTOR = (math.PI * 2.0) / FULL_DAY;
            return (float)((Value * TIME_FACTOR) % TWO_PI);
        }

        /// <summary>
        /// Direction of the sun based on the time of day.
        /// </summary>
        public float3 SunDir() {
            float angle = GetAngleRad();
            return new float3(-math.sin(angle), 0f, math.cos(angle));
        }

        /// <summary>
        /// Direction of the moon based on the time of day.
        /// </summary>
        public float3 MoonDir() {
            float angle = GetAngleRad();
            return -math.normalize(new float3(-math.sin(angle), 0f, math.cos(angle) - 0.5f));
        }

        public double Day => math.fmod(Value, FULL_DAY);
    }

    /// <summary>
    /// Time value in seconds used for game progression.
    /// </summary>
    [Serializable]
    public struct Time {
        public double Seconds;

        public Time(double seconds) { Seconds = seconds; }

        public Time AddSeconds(double secs) => new Time(Seconds + secs);
        public Time AddDays(double days, double dayCycleCoefficient) =>
            AddSeconds(days * 3600.0 * 24.0 / dayCycleCoefficient);
    }

    /// <summary>
    /// Real program time local to client or server.
    /// </summary>
    [Serializable]
    public struct ProgramTime {
        public double Seconds;
        public ProgramTime(double seconds) { Seconds = seconds; }
    }

    /// <summary>
    /// Time scaling factor.
    /// </summary>
    [Serializable]
    public struct TimeScale {
        public double Value;
        public TimeScale(double value) { Value = value; }
        public static TimeScale Default => new TimeScale(1.0);
    }

    /// <summary>
    /// Time since previous tick in seconds.
    /// </summary>
    [Serializable]
    public struct DeltaTime {
        public float Value;
        public DeltaTime(float value) { Value = value; }
    }

    /// <summary>
    /// Convenience wrapper used to indicate a duration of time in seconds.
    /// </summary>
    [Serializable]
    public struct Secs {
        public double Value;
        public Secs(double value) { Value = value; }
        public static Secs operator *(Secs secs, double mult) => new Secs(secs.Value * mult);
        public static Secs operator *(double mult, Secs secs) => secs * mult;
        public void MultiplyAssign(double mult) => Value *= mult;
    }
}
