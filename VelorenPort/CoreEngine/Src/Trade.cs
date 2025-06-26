using System;
using System.Collections.Generic;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Trade and economy related types. Partial port of <c>trade.rs</c>.
    /// Currently includes the <c>Good</c> union and helper methods.
    /// </summary>
    [Serializable]
    public abstract record Good {
        /// <summary>Default variant matching Rust's <c>Default</c>.</summary>
        public static Good Default => new Terrain(World.BiomeKind.Void);
        public sealed record Territory(World.BiomeKind Biome) : Good;
        public sealed record Flour : Good;
        public sealed record Meat : Good;
        public sealed record Terrain(World.BiomeKind Biome) : Good;
        public sealed record Transportation : Good;
        public sealed record Food : Good;
        public sealed record Wood : Good;
        public sealed record Stone : Good;
        public sealed record Tools : Good;
        public sealed record Armor : Good;
        public sealed record Ingredients : Good;
        public sealed record Potions : Good;
        public sealed record Coin : Good;
        public sealed record RoadSecurity : Good;
        public sealed record Recipe : Good;
    }

    public static class GoodExtensions {
        /// <summary>Discount factor when selling goods back to merchants.</summary>
        public static float TradeMargin(this Good good) => good switch {
            Good.Tools or Good.Armor => 0.5f,
            Good.Food or Good.Potions or Good.Ingredients or Good.Wood => 0.75f,
            Good.Coin or Good.Recipe => 1.0f,
            _ => 0.0f,
        };
    }

    /// <summary>Identifier of a trade site.</summary>
    using SiteId = System.UInt64;

    /// <summary>Information about available stock at a site.</summary>
    [Serializable]
    public record SiteInformation(SiteId Id, Dictionary<Good, float> UnconsumedStock);

    /// <summary>Mapping of good values at a site.</summary>
    [Serializable]
    public class SitePrices {
        public Dictionary<Good, float> Values { get; } = new();
        public void AddPrice(Good good, float value) => Values[good] = value;

        public float? Balance(Dictionary<InvSlotId, uint>[] offers, ReducedInventory?[] inventories, int who, bool reduce) {
            float total = 0f;
            foreach (var kv in offers[who]) {
                var slot = kv.Key;
                var amount = kv.Value;
                var optInv = inventories[who];
                if (optInv == null) return null;
                if (!optInv.Inventory.TryGetValue(slot, out var item)) return null;
                var materials = TradePricing.GetMaterials(item.Name);
                if (materials == null) return null;
                float sum = 0f;
                foreach (var (amt2, mat) in materials) {
                    Values.TryGetValue(mat, out var price);
                    sum += price * amt2 * (reduce ? mat.TradeMargin() : 1f);
                }
                total += sum * amount;
            }
            return total;
        }
    }

    [Serializable]
    public enum TradePhase {
        Mutate,
        Review,
        Complete,
    }

    public static class TradePhaseExtensions {
        public static TradePhase Next(this TradePhase phase) => phase switch {
            TradePhase.Mutate => TradePhase.Review,
            TradePhase.Review => TradePhase.Complete,
            _ => TradePhase.Complete,
        };
    }

    [Serializable]
    public enum TradeActionKind {
        AddItem,
        RemoveItem,
        Accept,
        Decline,
    }

    [Serializable]
    public record TradeAction {
        public TradeActionKind Kind { get; init; }
        public TradePhase Phase { get; init; }
        public InvSlotId Item { get; init; }
        public uint Quantity { get; init; }
        public bool Ours { get; init; }

        public static TradeAction Add(InvSlotId item, uint quantity, bool ours) =>
            new TradeAction { Kind = TradeActionKind.AddItem, Item = item, Quantity = quantity, Ours = ours };
        public static TradeAction Remove(InvSlotId item, uint quantity, bool ours) =>
            new TradeAction { Kind = TradeActionKind.RemoveItem, Item = item, Quantity = quantity, Ours = ours };
        public static TradeAction Accept(TradePhase phase) =>
            new TradeAction { Kind = TradeActionKind.Accept, Phase = phase };
        public static TradeAction Decline() => new TradeAction { Kind = TradeActionKind.Decline };

        public static TradeAction? Item(InvSlotId item, int delta, bool ours) => delta switch {
            0 => null,
            < 0 => Remove(item, (uint)(-delta), ours),
            _ => Add(item, (uint)delta, ours),
        };
    }

    [Serializable]
    public enum TradeResult {
        Completed,
        Declined,
        NotEnoughSpace,
    }

    [Serializable]
    public class PendingTrade {
        public Uid[] Parties { get; } = new Uid[2];
        public Dictionary<InvSlotId, uint>[] Offers { get; } = {
            new Dictionary<InvSlotId, uint>(), new Dictionary<InvSlotId, uint>() };
        public TradePhase Phase { get; private set; } = TradePhase.Mutate;
        public bool[] AcceptFlags { get; } = new bool[2];

        public PendingTrade(Uid party, Uid counterparty) {
            Parties[0] = party;
            Parties[1] = counterparty;
        }

        public TradePhase GetPhase() => Phase;
        public bool ShouldCommit() => Phase == TradePhase.Complete;

        public int? WhichParty(Uid party) {
            for (int i = 0; i < 2; i++) {
                if (Parties[i].Equals(party)) return i;
            }
            return null;
        }

        public bool IsEmptyTrade() => Offers[0].Count == 0 && Offers[1].Count == 0;

        public void ProcessTradeAction(int who, TradeAction action, IInventory[] inventories) {
            switch (action.Kind) {
                case TradeActionKind.AddItem:
                    if (Phase == TradePhase.Mutate && action.Quantity > 0) {
                        if (!action.Ours) who = 1 - who;
                        Offers[who].TryGetValue(action.Item, out var total);
                        var owned = inventories[who]?.GetAmount(action.Item) ?? 0;
                        Offers[who][action.Item] = Math.Min(total + action.Quantity, owned);
                        AcceptFlags[0] = AcceptFlags[1] = false;
                    }
                    break;
                case TradeActionKind.RemoveItem:
                    if (Phase == TradePhase.Mutate) {
                        if (!action.Ours) who = 1 - who;
                        if (Offers[who].TryGetValue(action.Item, out var total)) {
                            var newTotal = total > action.Quantity ? total - action.Quantity : 0;
                            if (newTotal > 0) Offers[who][action.Item] = newTotal; else Offers[who].Remove(action.Item);
                        }
                        AcceptFlags[0] = AcceptFlags[1] = false;
                    }
                    break;
                case TradeActionKind.Accept:
                    if (Phase == action.Phase && !IsEmptyTrade()) {
                        AcceptFlags[who] = true;
                    }
                    if (AcceptFlags[0] && AcceptFlags[1]) {
                        Phase = Phase.Next();
                        AcceptFlags[0] = AcceptFlags[1] = false;
                    }
                    break;
                case TradeActionKind.Decline:
                    break;
            }
        }
    }

    public readonly record struct TradeId(int Value);

    public class Trades {
        public TradeId NextId { get; private set; } = new TradeId(0);
        public Dictionary<TradeId, PendingTrade> Active { get; } = new();
        public Dictionary<Uid, TradeId> EntityTrades { get; } = new();

        public TradeId BeginTrade(Uid party, Uid counterparty) {
            var id = NextId;
            NextId = new TradeId(id.Value + 1);
            Active[id] = new PendingTrade(party, counterparty);
            EntityTrades[party] = id;
            EntityTrades[counterparty] = id;
            return id;
        }

        public void ProcessTradeAction(TradeId id, Uid who, TradeAction action, Func<Uid, IInventory?> getInventory) {
            if (!Active.TryGetValue(id, out var trade)) return;
            var party = trade.WhichParty(who);
            if (party is int p) {
                var inventories = new IInventory[2];
                for (int i = 0; i < 2; i++) {
                    var inv = getInventory(trade.Parties[i]);
                    if (inv == null) return;
                    inventories[i] = inv;
                }
                trade.ProcessTradeAction(p, action, inventories);
            }
        }

        public Uid? DeclineTrade(TradeId id, Uid who) {
            if (!Active.Remove(id, out var trade)) return null;
            var which = trade.WhichParty(who);
            if (which is int p) {
                EntityTrades.Remove(trade.Parties[0]);
                EntityTrades.Remove(trade.Parties[1]);
                return trade.Parties[1 - p];
            } else {
                Active[id] = trade;
                return null;
            }
        }

        private bool InTradeWith(Uid uid, Func<PendingTrade, bool> predicate) =>
            EntityTrades.TryGetValue(uid, out var id) &&
            Active.TryGetValue(id, out var trade) && predicate(trade);

        public bool InImmutableTrade(Uid uid) => InTradeWith(uid, t => t.GetPhase() != TradePhase.Mutate);
        public bool InMutableTrade(Uid uid) => InTradeWith(uid, t => t.GetPhase() == TradePhase.Mutate);

        public void ImplicitMutationOccurred(Uid uid) {
            if (EntityTrades.TryGetValue(uid, out var id) && Active.TryGetValue(id, out var trade)) {
                trade.AcceptFlags[0] = trade.AcceptFlags[1] = false;
            }
        }
    }
}
