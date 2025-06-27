using System;
using System.Collections.Generic;
using rand = System.Random;
using combat = VelorenPort.CoreEngine;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Definition of an explosion and related helper types. Ported from
    /// <c>explosion.rs</c>.
    /// </summary>
    [Serializable]
    public class Explosion {
        public List<RadiusEffect> Effects { get; set; } = new();
        public float Radius { get; set; }
        public comp.item.Reagent? Reagent { get; set; }
        public float MinFalloff { get; set; }
    }

    [Serializable]
    public abstract record RadiusEffect {
        [Serializable]
        public sealed record TerrainDestruction(float Strength, Rgb<float> Color) : RadiusEffect;
        [Serializable]
        public sealed record Entity(Effect Effect) : RadiusEffect;
        [Serializable]
        public sealed record Attack(combat.Attack Attack) : RadiusEffect;
    }

    [Serializable]
    public enum ColorPreset {
        Black,
        InkBomb,
        IceBomb,
    }

    public static class ColorPresetExtensions {
        public static Rgb<float> ToRgb(this ColorPreset preset)
        {
            switch (preset)
            {
                case ColorPreset.Black:
                    return new Rgb<float>(0f, 0f, 0f);
                case ColorPreset.InkBomb:
                    return new Rgb<float>(4f, 7f, 32f);
                case ColorPreset.IceBomb:
                    var rng = new rand();
                    float variation = (float)rng.NextDouble();
                    return new Rgb<float>(
                        83f - 20f * variation,
                        212f - 52f * variation,
                        255f - 62f * variation
                    );
                default:
                    return new Rgb<float>(0f, 0f, 0f);
            }
        }
    }
}
