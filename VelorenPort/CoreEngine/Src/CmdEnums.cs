using System;
using System.Collections.Generic;

namespace VelorenPort.CoreEngine {
    /// <summary>Specification of starter equipment.</summary>
    [Serializable]
    public abstract record KitSpec {
        [Serializable] public sealed record Item(string Name) : KitSpec;
        [Serializable] public sealed record ModularWeaponSet(string Tool, string Material, string? Hands) : KitSpec;
        [Serializable] public sealed record ModularWeaponRandom(string Tool, string Material, string? Hands) : KitSpec;
    }

    /// <summary>Area permission categories.</summary>
    [Serializable]
    public enum AreaKind {
        Build,
        NoDurability
    }

    /// <summary>Target reference used in admin commands.</summary>
    [Serializable]
    public abstract record EntityTarget {
        [Serializable] public sealed record Player(string Name) : EntityTarget;
        [Serializable] public sealed record RtsimNpc(ulong Id) : EntityTarget;
        [Serializable] public sealed record Uid(VelorenPort.CoreEngine.Uid Id) : EntityTarget;
    }

    /// <summary>Indicates whether a command argument is required.</summary>
    [Serializable]
    public enum Requirement {
        Required,
        Optional
    }

    /// <summary>Chat command argument specification.</summary>
    [Serializable]
    public abstract record ArgumentSpec {
        public abstract Requirement Requirement { get; }
        public abstract string UsageString();

        [Serializable] public sealed record PlayerName(Requirement Req) : ArgumentSpec {
            public override Requirement Requirement => Req;
            public override string UsageString() => Req == Requirement.Required ? "<player>" : "[player]";
        }
        [Serializable] public sealed record Entity(Requirement Req) : ArgumentSpec {
            public override Requirement Requirement => Req;
            public override string UsageString() => Req == Requirement.Required ? "<entity>" : "[entity]";
        }
        [Serializable] public sealed record SiteName(Requirement Req) : ArgumentSpec {
            public override Requirement Requirement => Req;
            public override string UsageString() => Req == Requirement.Required ? "<site>" : "[site]";
        }
        [Serializable] public sealed record Float(string Label, float Suggest, Requirement Req) : ArgumentSpec {
            public override Requirement Requirement => Req;
            public override string UsageString() => Req == Requirement.Required ? $"<{Label}>" : $"[{Label}]";
        }
        [Serializable] public sealed record Integer(string Label, int Suggest, Requirement Req) : ArgumentSpec {
            public override Requirement Requirement => Req;
            public override string UsageString() => Req == Requirement.Required ? $"<{Label}>" : $"[{Label}]";
        }
        [Serializable] public sealed record Any(string Label, Requirement Req) : ArgumentSpec {
            public override Requirement Requirement => Req;
            public override string UsageString() => Req == Requirement.Required ? $"<{Label}>" : $"[{Label}]";
        }
        [Serializable] public sealed record Command(Requirement Req) : ArgumentSpec {
            public override Requirement Requirement => Req;
            public override string UsageString() => Req == Requirement.Required ? "<[/]command>" : "[[/]command]";
        }
        [Serializable] public sealed record Message(Requirement Req) : ArgumentSpec {
            public override Requirement Requirement => Req;
            public override string UsageString() => Req == Requirement.Required ? "<message>" : "[message]";
        }
        [Serializable] public sealed record SubCommand() : ArgumentSpec {
            public override Requirement Requirement => Requirement.Required;
            public override string UsageString() => "<[/]command> [args...]";
        }
        [Serializable] public sealed record Enum(string Label, List<string> Options, Requirement Req) : ArgumentSpec {
            public override Requirement Requirement => Req;
            public override string UsageString() => Req == Requirement.Required ? $"<{Label}>" : $"[{Label}]";
        }
        [Serializable] public sealed record AssetPath(string Label, string Prefix, List<string> Paths, Requirement Req) : ArgumentSpec {
            public override Requirement Requirement => Req;
            public override string UsageString() => Req == Requirement.Required ? $"<{Label}>" : $"[{Label}]";
        }
        [Serializable] public sealed record Boolean(string Label, string Completion, Requirement Req) : ArgumentSpec {
            public override Requirement Requirement => Req;
            public override string UsageString() => Req == Requirement.Required ? $"<{Label}>" : $"[{Label}]";
        }
        [Serializable] public sealed record Flag(string Label) : ArgumentSpec {
            public override Requirement Requirement => Requirement.Optional;
            public override string UsageString() => $"[{Label}]";
        }
    }
}
