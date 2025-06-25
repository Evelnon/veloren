using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Weather data and helper utilities mirrored from common/src/weather.rs
    /// </summary>
    [Serializable]
    public struct Weather {
        public float Cloud;
        public float Rain;
        public float2 Wind;

        public Weather(float cloud, float rain, float2 wind) {
            Cloud = cloud;
            Rain = rain;
            Wind = wind;
        }

        public WeatherKind GetKind() {
            if (math.lengthsq(Wind) >= 24.5f * 24.5f) return WeatherKind.Storm;
            if (Rain >= 0.1f && Rain <= 1.0f) return WeatherKind.Rain;
            if (Cloud >= 0.2f && Cloud <= 1.0f) return WeatherKind.Cloudy;
            return WeatherKind.Clear;
        }

        public Weather LerpUnclamped(Weather to, float t) =>
            new Weather(math.lerp(Cloud, to.Cloud, t),
                         math.lerp(Rain, to.Rain, t),
                         math.lerp(Wind, to.Wind, t));

        public static Weather LerpUnclamped(in Weather a, in Weather b, float t) => a.LerpUnclamped(b, t);

        public float3 RainVel() {
            const float FALL_RATE = 30f;
            return new float3(Wind.x, Wind.y, -FALL_RATE);
        }

        public float2 WindVel() => Wind;
    }

    public enum WeatherKind {
        Clear,
        Cloudy,
        Rain,
        Storm,
    }

    public static class WeatherKindExtensions {
        public static string Display(this WeatherKind kind) => kind switch {
            WeatherKind.Clear => "Clear",
            WeatherKind.Cloudy => "Cloudy",
            WeatherKind.Rain => "Rain",
            WeatherKind.Storm => "Storm",
            _ => kind.ToString()
        };
    }

    public static class WeatherConsts {
        public const uint CHUNKS_PER_CELL = 16;
        public const uint CELL_SIZE = CHUNKS_PER_CELL * (uint)TerrainConstants.ChunkSize.x;
    }

    [Serializable]
    public struct CompressedWeather {
        public byte Cloud;
        public byte Rain;

        public CompressedWeather(byte cloud, byte rain) {
            Cloud = cloud;
            Rain = rain;
        }

        public Weather LerpUnclamped(CompressedWeather to, float t) =>
            new Weather(
                math.lerp(Cloud, to.Cloud, t) / 255f,
                math.lerp(Rain, to.Rain, t) / 255f,
                float2.zero);

        public static CompressedWeather FromWeather(Weather w) => new CompressedWeather(
            (byte)math.round(math.clamp(w.Cloud, 0f, 1f) * 255f),
            (byte)math.round(math.clamp(w.Rain, 0f, 1f) * 255f));

        public static Weather ToWeather(CompressedWeather cw) => new Weather(cw.Cloud / 255f, cw.Rain / 255f, float2.zero);
    }

    [Serializable]
    public class WeatherGrid {
        private Grid<Weather> _weather;

        public WeatherGrid(int2 size) {
            _weather = new Grid<Weather>(size, default);
        }

        internal WeatherGrid(Grid<Weather> grid) { _weather = grid; }

        public int2 Size => _weather.Size;

        public IEnumerable<(int2 Pos, Weather Value)> Iterate() => _weather.Iterate();

        public Weather Get(int2 cellPos) => _weather.Get(cellPos) ?? default;

        public Weather GetInterpolated(float2 wpos) {
            float2 cell = wpos / WeatherConsts.CELL_SIZE - 0.5f;
            float2 rpos = math.frac(cell) + (1f - math.sign(cell)) / 2f;
            int2 cpos = (int2)math.floor(cell);

            Weather w00 = _weather.Get(cpos) ?? default;
            Weather w10 = _weather.Get(cpos + new int2(1,0)) ?? default;
            Weather w01 = _weather.Get(cpos + new int2(0,1)) ?? default;
            Weather w11 = _weather.Get(cpos + new int2(1,1)) ?? default;

            var wx0 = Weather.LerpUnclamped(in w00, in w10, rpos.x);
            var wx1 = Weather.LerpUnclamped(in w01, in w11, rpos.x);
            return Weather.LerpUnclamped(in wx0, in wx1, rpos.y);
        }

        public Weather GetMaxNear(float2 wpos) {
            int2 cellPos = (int2)math.floor(wpos / WeatherConsts.CELL_SIZE - 0.5f);
            Weather? max = null;
            foreach (var offset in WeatherGridHelper.LOCALITY) {
                Weather w = _weather.Get(cellPos + offset) ?? default;
                if (max is null) { max = w; continue; }
                var val = max.Value;
                val.Cloud = math.max(val.Cloud, w.Cloud);
                val.Rain = math.max(val.Rain, w.Rain);
                val.Wind = math.max(val.Wind, w.Wind);
                max = val;
            }
            return max ?? default;
        }

        internal Grid<Weather> Raw => _weather;
    }

    [Serializable]
    public class SharedWeatherGrid {
        private Grid<CompressedWeather> _weather;

        public SharedWeatherGrid(int2 size) {
            _weather = new Grid<CompressedWeather>(size, default);
        }

        internal SharedWeatherGrid(Grid<CompressedWeather> grid) { _weather = grid; }

        public IEnumerable<(int2 Pos, CompressedWeather Value)> Iterate() => _weather.Iterate();

        public int2 Size => _weather.Size;

        public static SharedWeatherGrid FromWeatherGrid(WeatherGrid grid) {
            var raw = new List<CompressedWeather>();
            foreach (var (_, w) in grid.Raw.Iterate())
                raw.Add(CompressedWeather.FromWeather(w));
            return new SharedWeatherGrid(new Grid<CompressedWeather>(grid.Size, raw));
        }

        public WeatherGrid ToWeatherGrid() {
            var raw = new List<Weather>();
            foreach (var (_, cw) in _weather.Iterate())
                raw.Add(CompressedWeather.ToWeather(cw));
            return new WeatherGrid(new Grid<Weather>(_weather.Size, raw));
        }
    }

    internal static class WeatherGridHelper {
        public static readonly int2[] LOCALITY = new[] {
            new int2(0,0), new int2(0,1), new int2(1,0), new int2(0,-1), new int2(-1,0),
            new int2(1,1), new int2(1,-1), new int2(-1,1), new int2(-1,-1)
        };
    }
}
