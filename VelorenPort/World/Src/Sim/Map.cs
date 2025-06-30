using System;
using VelorenPort.NativeMath;
using VelorenPort.CoreEngine;

namespace VelorenPort.World.Sim
{
    /// <summary>
    /// Configuration for map sampling. Mirrors a subset of the Rust MapConfig
    /// structure used when rendering world overview maps.
    /// </summary>
    [Serializable]
    public struct MapConfig
    {
        public MapSizeLg MapSizeLg;
        public int2 Dimensions;
        public double3 Focus;
        public float Gain;

        public bool IsBasement;
        public bool IsWater;
        public bool IsIce;
        public bool IsShaded;
        public bool IsTemperature;
        public bool IsHumidity;

        public static MapConfig Orthographic(MapSizeLg mapSizeLg, float startZ, float endZ)
        {
            return new MapConfig
            {
                MapSizeLg = mapSizeLg,
                Dimensions = mapSizeLg.Chunks,
                Focus = new double3(0, 0, startZ),
                Gain = endZ - startZ,
                IsBasement = false,
                IsWater = true,
                IsIce = true,
                IsShaded = true,
                IsTemperature = false,
                IsHumidity = false,
            };
        }

        public MapSizeLg GetMapSizeLg() => MapSizeLg;
    }

    /// <summary>
    /// Kind of connection between two chunks.
    /// Currently only rivers are represented.
    /// </summary>
    public enum ConnectionKind
    {
        River,
        Path
    }

    /// <summary>
    /// Connection metadata for a neighboring chunk edge.
    /// </summary>
    public struct Connection
    {
        public ConnectionKind Kind;
        public float2 SplineDerivative;
        public float Width;

        public Connection(ConnectionKind kind, float2 derivative, float width)
        {
            Kind = kind;
            SplineDerivative = derivative;
            Width = width;
        }
    }

    /// <summary>
    /// Minimal per-chunk sample used by map generation.
    /// </summary>
    public struct MapSample
    {
        public Rgb<byte> Rgb;
        public double Alt;
        public int2 DownhillWpos;
        public Connection?[]? Connections;
    }

    /// <summary>
    /// Helper sampling utilities inspired by world/src/sim/map.rs.
    /// </summary>
    public static class Map
    {
        public static float SampleWpos(MapConfig cfg, WorldSim sim, int2 wpos)
        {
            var chunk = sim.GetWpos(wpos);
            float alt = WorldDefaults.CONFIG.SeaLevel;
            if (chunk != null)
            {
                alt = cfg.IsBasement ? chunk.Basement : chunk.Alt;
                if (cfg.IsWater)
                    alt = Math.Max(alt, chunk.WaterAlt);
            }

            return (alt - (float)cfg.Focus.z) / cfg.Gain;
        }

        public static MapSample SamplePos(MapConfig cfg, WorldSim sim, int2 cpos)
        {
            var chunk = sim.Get(cpos);
            float alt = WorldDefaults.CONFIG.SeaLevel;
            float basement = alt;
            float waterAlt = alt;
            int2 downhill = TerrainChunkSize.CposToWpos(cpos + int2.one);
            RiverKind? riverKind = null;
            float2 spline = float2.zero;
            if (chunk != null)
            {
                alt = chunk.Alt;
                basement = chunk.Basement;
                waterAlt = chunk.WaterAlt;
                downhill = chunk.Downhill ?? downhill;
                riverKind = chunk.River.Kind;
                spline = chunk.River.SplineDerivative;
            }

            float chosenAlt = cfg.IsBasement ? basement : alt;
            float normAlt = (chosenAlt - (float)cfg.Focus.z) / cfg.Gain;
            float normWater = (Math.Max(chosenAlt, waterAlt) - (float)cfg.Focus.z) / cfg.Gain;
            normAlt = math.clamp(normAlt, 0f, 1f);
            normWater = math.clamp(normWater, 0f, 1f);

            byte shade = (byte)(normAlt * 255);
            var rgb = new Rgb<byte>(shade, shade, shade);

            Connection?[]? connections = null;
            bool hasConnections = false;
            connections = new Connection?[8];
            if (riverKind == RiverKind.River && cfg.IsWater)
            {
                int2 dpos = TerrainChunkSize.WposToCpos(downhill);
                for (int i = 0; i < WorldUtil.NEIGHBORS.Length; i++)
                {
                    if (dpos - cpos == WorldUtil.NEIGHBORS[i])
                    {
                        connections[i] = new Connection(
                            ConnectionKind.River,
                            spline,
                            TerrainChunkSize.RectSize.x);
                        hasConnections = true;
                    }
                }
            }

            if (chunk != null && chunk.Path.way.IsWay)
            {
                for (int i = 0; i < WorldUtil.NEIGHBORS.Length; i++)
                {
                    if ((chunk.Path.way.Neighbors & (1 << i)) != 0)
                    {
                        connections[i] = new Connection(
                            ConnectionKind.Path,
                            float2.zero,
                            chunk.Path.path.Width);
                        hasConnections = true;
                    }
                }
            }

            if (!hasConnections)
                connections = null;

            double finalAlt = cfg.IsWater ? Math.Max(normAlt, normWater) : normAlt;

            return new MapSample
            {
                Rgb = rgb,
                Alt = finalAlt,
                DownhillWpos = downhill,
                Connections = connections
            };
        }
    }
}
