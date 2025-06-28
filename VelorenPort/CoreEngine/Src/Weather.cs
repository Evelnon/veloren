using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

namespace VelorenPort.CoreEngine
{
    /// <summary>Minimal weather representation.</summary>
    [Serializable]
    public struct Weather
    {
        public float Cloud;
        public float Rain;
        public float2 Wind;

        public Weather(float cloud, float rain, float2 wind)
        {
            Cloud = cloud;
            Rain = rain;
            Wind = wind;
        }

        public WeatherKind GetKind()
        {
            // Over 24.5 m/s is a storm
            if (math.lengthsq(Wind) >= 24.5f * 24.5f)
                return WeatherKind.Storm;
            if (Rain is >= 0.1f and <= 1.0f)
                return WeatherKind.Rain;
            if (Cloud is >= 0.2f and <= 1.0f)
                return WeatherKind.Cloudy;
            return WeatherKind.Clear;
        }

        public Weather LerpUnclamped(in Weather to, float t)
        {
            return new Weather(
                math.lerp(Cloud, to.Cloud, t),
                math.lerp(Rain, to.Rain, t),
                math.lerp(Wind, to.Wind, t));
        }

        public float3 RainVel()
        {
            const float fallRate = 30f;
            return new float3(Wind, -fallRate);
        }

        public float2 WindVel() => Wind;
    }

    public enum WeatherKind { Clear, Cloudy, Rain, Storm }

    [Serializable]
    public struct CompressedWeather
    {
        public byte Cloud;
        public byte Rain;

        public Weather LerpUnclamped(in CompressedWeather to, float t)
        {
            return new Weather(
                math.lerp(Cloud, to.Cloud, t) / 255f,
                math.lerp(Rain, to.Rain, t) / 255f,
                float2.zero);
        }

        public static implicit operator CompressedWeather(Weather w)
        {
            return new CompressedWeather
            {
                Cloud = (byte)math.round(math.clamp(w.Cloud, 0f, 1f) * 255f),
                Rain = (byte)math.round(math.clamp(w.Rain, 0f, 1f) * 255f)
            };
        }

        public static implicit operator Weather(CompressedWeather cw)
        {
            return new Weather(cw.Cloud / 255f, cw.Rain / 255f, float2.zero);
        }
    }

    /// <summary>Grid of weather cells with interpolation helpers.</summary>
    [Serializable]
    public class WeatherGrid
    {
        public const uint ChunksPerCell = 16;
        public const uint CellSize = ChunksPerCell * (uint)TerrainConstants.ChunkSize.x;

        private readonly Grid<Weather> _weather;

        // Locality offsets for GetMaxNear
        private static readonly int2[] Locality =
        {
            new int2(0, 0),
            new int2(0, 1),
            new int2(1, 0),
            new int2(0, -1),
            new int2(-1, 0),
            new int2(1, 1),
            new int2(1, -1),
            new int2(-1, 1),
            new int2(-1, -1)
        };

        public WeatherGrid(int2 size)
        {
            _weather = new Grid<Weather>(size, default);
        }

        public int2 Size => _weather.Size;

        public IEnumerable<(int2 Pos, Weather Cell)> Iterate() => _weather.Iterate();

        public Weather Get(int2 cellPos) => _weather.Get(cellPos) ?? default;

        private static float2 ToCellPos(float2 wpos) =>
            wpos / CellSize - new float2(0.5f, 0.5f);

        public Weather GetInterpolated(float2 worldPos)
        {
            var cellPos = ToCellPos(worldPos);
            var frac = new float2(cellPos.x - math.floor(cellPos.x), cellPos.y - math.floor(cellPos.y));
            frac += (1f - new float2(MathF.Sign(cellPos.x), MathF.Sign(cellPos.y))) * 0.5f;
            var basePos = new int2((int)math.floor(cellPos.x), (int)math.floor(cellPos.y));

            Weather w00 = Get(basePos);
            Weather w10 = Get(basePos + new int2(1, 0));
            Weather w01 = Get(basePos + new int2(0, 1));
            Weather w11 = Get(basePos + new int2(1, 1));

            Weather a = w00.LerpUnclamped(in w10, frac.x);
            Weather b = w01.LerpUnclamped(in w11, frac.x);
            return a.LerpUnclamped(in b, frac.y);
        }

        public Weather GetMaxNear(float2 worldPos)
        {
            var basePos = ToCellPos(worldPos);
            var cell = new int2((int)math.floor(basePos.x), (int)math.floor(basePos.y));
            Weather result = default;
            foreach (var offset in Locality)
            {
                var w = Get(cell + offset);
                result.Cloud = math.max(result.Cloud, w.Cloud);
                result.Rain = math.max(result.Rain, w.Rain);
                result.Wind = new float2(
                    math.max(result.Wind.x, w.Wind.x),
                    math.max(result.Wind.y, w.Wind.y));
            }
            return result;
        }

        public bool Set(int2 cellPos, Weather value) => _weather.Set(cellPos, value);
    }

    [Serializable]
    public class SharedWeatherGrid
    {
        private readonly Grid<CompressedWeather> _weather;

        public SharedWeatherGrid(int2 size)
        {
            _weather = new Grid<CompressedWeather>(size, default);
        }

        public int2 Size => _weather.Size;

        public IEnumerable<(int2 Pos, CompressedWeather Cell)> Iterate() => _weather.Iterate();

        public static SharedWeatherGrid FromWeatherGrid(WeatherGrid grid)
        {
            var res = new SharedWeatherGrid(grid.Size);
            foreach (var (pos, cell) in grid.Iterate())
                res._weather.Set(pos, (CompressedWeather)cell);
            return res;
        }

        public WeatherGrid ToWeatherGrid()
        {
            var res = new WeatherGrid(Size);
            foreach (var (pos, cell) in _weather.Iterate())
                res.Set(pos, (Weather)cell);
            return res;
        }
    }
}
