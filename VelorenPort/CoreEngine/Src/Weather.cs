using System;
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
    }

    public enum WeatherKind { Clear, Cloudy, Rain, Storm }

    /// <summary>Lightweight weather grid placeholder.</summary>
    [Serializable]
    public class WeatherGrid
    {
        private readonly int2 _size;
        public WeatherGrid(int2 size) { _size = size; }
        public int2 Size => _size;
    }
}
