using System;
using VelorenPort.CoreEngine;

namespace VelorenPort.World {
    /// <summary>
    /// Game world configuration mirrored from <c>world/src/config.rs</c>.
    /// </summary>
    [Serializable]
    public class Config {
        public float SeaLevel { get; init; }
        public float MountainScale { get; init; }
        public float SnowTemp { get; init; }
        public float TemperateTemp { get; init; }
        public float TropicalTemp { get; init; }
        public float DesertTemp { get; init; }
        public float DesertHum { get; init; }
        public float ForestHum { get; init; }
        public float JungleHum { get; init; }
        public float RainfallChunkRate { get; init; }
        public float RiverRoughness { get; init; }
        public float RiverMaxWidth { get; init; }
        public float RiverMinHeight { get; init; }
        public float RiverWidthToDepth { get; init; }
        public Rgb<byte> IceColor { get; init; }

        public Config() {}

        public Config(float seaLevel, float mountainScale, float snowTemp,
                      float temperateTemp, float tropicalTemp, float desertTemp,
                      float desertHum, float forestHum, float jungleHum,
                      float rainfallChunkRate, float riverRoughness,
                      float riverMaxWidth, float riverMinHeight,
                      float riverWidthToDepth, Rgb<byte> iceColor) {
            SeaLevel = seaLevel;
            MountainScale = mountainScale;
            SnowTemp = snowTemp;
            TemperateTemp = temperateTemp;
            TropicalTemp = tropicalTemp;
            DesertTemp = desertTemp;
            DesertHum = desertHum;
            ForestHum = forestHum;
            JungleHum = jungleHum;
            RainfallChunkRate = rainfallChunkRate;
            RiverRoughness = riverRoughness;
            RiverMaxWidth = riverMaxWidth;
            RiverMinHeight = riverMinHeight;
            RiverWidthToDepth = riverWidthToDepth;
            IceColor = iceColor;
        }
    }

    /// <summary>
    /// Feature flags controlling which generators run when creating a world.
    /// </summary>
    [Serializable]
    public class Features {
        public bool Caverns { get; set; }
        public bool Caves { get; set; }
        public bool Rocks { get; set; }
        public bool Shrubs { get; set; }
        public bool Trees { get; set; }
        public bool Scatter { get; set; }
        public bool Paths { get; set; }
        public bool Spots { get; set; }
        public float WildlifeDensity { get; set; }
        public bool PeakNaming { get; set; }
        public bool BiomeNaming { get; set; }
        public bool TrainTracks { get; set; }
    }

    /// <summary>
    /// Default world configuration equivalent to the constant <c>CONFIG</c>
    /// in the Rust version.
    /// </summary>
    public static class WorldDefaults {
        public static readonly Config CONFIG = new Config {
            SeaLevel = 140f,
            MountainScale = 2048f,
            SnowTemp = -0.8f,
            TemperateTemp = -0.4f,
            TropicalTemp = 0.4f,
            DesertTemp = 0.8f,
            DesertHum = 0.15f,
            ForestHum = 0.5f,
            JungleHum = 0.75f,
            RainfallChunkRate = 1f / (512f * 32f * 32f),
            RiverRoughness = 0.06125f,
            RiverMaxWidth = 2f,
            RiverMinHeight = 0.25f,
            RiverWidthToDepth = 8f,
            IceColor = new Rgb<byte>(140, 175, 255),
        };
    }
}
