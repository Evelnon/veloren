using System.Collections.Generic;
using VelorenPort.CoreEngine;

namespace VelorenPort.Server.Weather
{
    /// <summary>
    /// Represents a queued weather update. In the original server this would
    /// update cloud cover, rain and other effects. Here we simply keep a time
    /// until the next weather tick.
    /// </summary>
    public class WeatherJob
    {
        /// <summary>Optional callback invoked whenever the weather changes.</summary>
        public System.Action<Weather>? WeatherChanged { get; set; }

        /// <summary>Configuration for client side visual effects.</summary>
        public WeatherEffects Effects { get; } = new();

        /// <summary>Simple physics model parameters.</summary>
        public WeatherPhysics Physics { get; } = new();

        /// <summary>When the next weather update should occur.</summary>
        public System.DateTime NextUpdate { get; set; }

        private readonly Queue<WeatherZone> _zones = new();

        private Weather? _target;
        private Weather _start;
        private System.DateTime _transitionStart;
        private System.DateTime _transitionEnd;

        /// <summary>Add a temporary weather zone lasting for <paramref name="duration"/>.</summary>
        public void QueueZone(Weather weather, System.TimeSpan duration) =>
            _zones.Enqueue(new WeatherZone(weather, System.DateTime.UtcNow + duration));

        /// <summary>Remove all queued zones. Mostly used for testing.</summary>
        public void ClearZones() => _zones.Clear();

        /// <summary>Returns true if a zone is currently active.</summary>
        public bool HasZone => _zones.Count > 0;

        /// <summary>Returns true if a transition is currently in progress.</summary>
        public bool IsTransitioning => _target.HasValue;

        /// <summary>
        /// Begin smoothly transitioning to <paramref name="weather"/> over <paramref name="duration"/>.
        /// </summary>
        public void StartTransition(Weather weather, System.TimeSpan duration, Weather current, System.DateTime? now = null)
        {
            _target = weather;
            _start = current;
            _transitionStart = now ?? System.DateTime.UtcNow;
            _transitionEnd = _transitionStart + duration;
        }

        /// <summary>
        /// Update active zones and optionally replace <paramref name="currentWeather"/>.
        /// Returns true if the weather was changed.
        /// </summary>
        public bool Tick(ref Weather currentWeather)
        {
            bool changed = false;
            var now = System.DateTime.UtcNow;
            while (_zones.Count > 0 && _zones.Peek().ExpiresAt <= now)
                _zones.Dequeue();

            if (_zones.Count > 0)
            {
                var zone = _zones.Peek();
                if (!currentWeather.Equals(zone.Weather))
                {
                    currentWeather = zone.Weather;
                    _target = null;
                    changed = true;
                }
            }
            else if (_target.HasValue)
            {
                if (now >= _transitionEnd)
                {
                    if (!currentWeather.Equals(_target.Value))
                        currentWeather = _target.Value;
                    _target = null;
                    changed = true;
                }
                else
                {
                    float t = (float)((now - _transitionStart).TotalSeconds / (_transitionEnd - _transitionStart).TotalSeconds);
                    currentWeather = _start.LerpUnclamped(in _target.Value, t);
                    changed = true;
                }
            }

            if (changed)
                WeatherChanged?.Invoke(currentWeather);

            return changed;
        }
    }

    /// <summary>Zone with custom weather lasting until <see cref="ExpiresAt"/>.</summary>
    public readonly struct WeatherZone
    {
        public Weather Weather { get; }
        public System.DateTime ExpiresAt { get; }

        public WeatherZone(Weather weather, System.DateTime expiresAt)
        {
            Weather = weather;
            ExpiresAt = expiresAt;
        }
    }

    /// <summary>Visual effect toggles for weather rendering.</summary>
    public class WeatherEffects
    {
        public bool EnableRain { get; set; } = true;
        public bool EnableSnow { get; set; } = true;
        public float CloudDensity { get; set; } = 1f;
    }

    /// <summary>Parameters controlling simple physics of rain and wind.</summary>
    public class WeatherPhysics
    {
        public float WindResistance { get; set; } = 0.1f;
        public float GravityScale { get; set; } = 1f;
    }
}
