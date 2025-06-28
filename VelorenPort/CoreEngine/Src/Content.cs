using System;
using System.Collections.Generic;
using System.Linq;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Representation of localisable content. Mirrors `common/i18n` crate.
    /// </summary>
    [Serializable]
    public abstract record Content {
        [Serializable]
        public sealed record Plain(string Text) : Content;

        [Serializable]
        public sealed record Key(string Value) : Content;

        [Serializable]
        public sealed record Attr(string KeyName, string Attribute) : Content;

        [Serializable]
        public sealed record Localized(string KeyName, ushort Seed, Dictionary<string, LocalizationArg> Args) : Content;

        private static ushort RandomSeed() => (ushort)Random.Shared.Next(0, ushort.MaxValue + 1);

        public static Content Dummy() => new Plain(string.Empty);

        [Obsolete]
        public static Content Legacy(string text) => new Plain(text);

        public static Content LocalizedMsg(string key) => new Localized(key, RandomSeed(), new());

        public static Content WithAttr(string key, string attr) => new Attr(key, attr);

        public static Content LocalizedWithArgs(string key, IEnumerable<(string Key, LocalizationArg Value)> args)
        {
            return new Localized(key, RandomSeed(), args.ToDictionary(a => a.Key, a => a.Value));
        }

        public string? AsPlain() => this is Plain p ? p.Text : null;
    }

    /// <summary>
    /// Localisation argument used in <see cref="Content.Localized"/>.
    /// </summary>
    [Serializable]
    public abstract record LocalizationArg {
        [Serializable]
        public sealed record ContentArg(Content Content) : LocalizationArg;

        [Serializable]
        public sealed record Nat(ulong Value) : LocalizationArg;

        public static implicit operator LocalizationArg(Content content) => new ContentArg(content);
        public static implicit operator LocalizationArg(string text) => new ContentArg(new Content.Plain(text));
        public static implicit operator LocalizationArg(ulong value) => new Nat(value);
    }
}
