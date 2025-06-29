using System.Collections.Generic;
using Unity.Mathematics;
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

        /// <summary>Invoked when visual parameters change.</summary>
        public System.Action<WeatherEffects>? EffectsChanged { get; set; }

        /// <summary>Configuration for client side visual effects.</summary>
        public WeatherEffects Effects { get; } = new();

        /// <summary>Simple physics model parameters.</summary>
        public WeatherPhysics Physics { get; } = new();

        /// <summary>When the next weather update should occur.</summary>
        public System.DateTime NextUpdate { get; set; }

        private readonly Queue<WeatherZone> _zones = new();
        private readonly Queue<QueuedZone> _queued = new();
        private readonly object _lock = new();

        private Weather? _target;
        private Weather _start;
        private System.DateTime _transitionStart;
        private System.DateTime _transitionEnd;

        /// <summary>Add a temporary weather zone lasting for <paramref name="duration"/>.</summary>
        public void QueueZone(Weather weather, System.TimeSpan duration)
        {
            lock (_lock)
                _zones.Enqueue(new WeatherZone(weather, System.DateTime.UtcNow + duration));
        }

        public void QueueZone(Weather weather, float2 pos, float radius, float duration)
        {
            lock (_lock)
                _queued.Enqueue(new QueuedZone(weather, pos, radius, duration));
        }

        /// <summary>Remove all queued zones. Mostly used for testing.</summary>
        public void ClearZones()
        {
            lock (_lock)
                _zones.Clear();
        }

        /// <summary>Returns true if a zone is currently active.</summary>
        public bool HasZone { get { lock (_lock) { return _zones.Count > 0; } } }

        /// <summary>Returns true if a transition is currently in progress.</summary>
        public bool IsTransitioning { get { lock (_lock) { return _target.HasValue; } } }

        /// <summary>
        /// Begin smoothly transitioning to <paramref name="weather"/> over <paramref name="duration"/>.
        /// </summary>
        public void StartTransition(Weather weather, System.TimeSpan duration, Weather current, System.DateTime? now = null)
        {
            lock (_lock)
            {
                _target = weather;
                _start = current;
                _transitionStart = now ?? System.DateTime.UtcNow;
                _transitionEnd = _transitionStart + duration;
            }
        }

        /// <summary>
        /// Update active zones and optionally replace <paramref name="currentWeather"/>.
        /// Returns true if the weather was changed.
        /// </summary>
        public bool Tick(ref Weather currentWeather)
        {
            bool changed = false;
            lock (_lock)
            {
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
            }

            if (changed)
            {
                Effects.Wind = currentWeather.Wind;
                Effects.PrecipitationStrength = currentWeather.Rain;
                Effects.CloudLayers = new float3(currentWeather.Cloud,
                                                currentWeather.Cloud,
                                                currentWeather.Cloud);
                WeatherChanged?.Invoke(currentWeather);
                EffectsChanged?.Invoke(Effects);
            }

            return changed;
        }

        /// <summary>Return any queued zones and clear the queue.</summary>
        public IEnumerable<QueuedZone> DrainQueuedZones()
        {
            lock (_lock)
            {
                while (_queued.Count > 0)
                    yield return _queued.Dequeue();
            }
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

    public readonly struct QueuedZone
    {
        public Weather Weather { get; }
        public float2 Pos { get; }
        public float Radius { get; }
        public float Time { get; }

        public QueuedZone(Weather weather, float2 pos, float radius, float time)
        {
            Weather = weather;
            Pos = pos;
            Radius = radius;
            Time = time;
        }
    }

    /// <summary>Visual effect toggles for weather rendering.</summary>
    public class WeatherEffects
    {
        public bool EnableRain { get; set; } = true;
        public bool EnableSnow { get; set; } = true;
        public float CloudDensity { get; set; } = 1f;

        public float2 Wind { get; set; } = float2.zero;
        public float PrecipitationStrength { get; set; }
        public float3 CloudLayers { get; set; } = float3.zero;
    }

    /// <summary>Parameters controlling simple physics of rain and wind.</summary>
    public class WeatherPhysics
    {
        public float WindResistance { get; set; } = 0.1f;
        public float GravityScale { get; set; } = 1f;

        /// <summary>Calculate wind velocity accounting for resistance.</summary>
        public float2 WindVelocity(float2 wind)
        {
            return wind * (1f - WindResistance);
        }

        /// <summary>Calculate rain particle velocity.</summary>
        public float3 RainVelocity(float2 wind)
        {
            const float FallRate = 30f;
            return new float3(wind, -FallRate * GravityScale);
        }
    }
}
