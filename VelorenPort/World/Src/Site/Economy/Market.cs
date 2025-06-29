using System;
using System.Collections.Generic;
using System.Linq;

namespace VelorenPort.World.Site.Economy
{
    /// <summary>
    /// Simple market allowing sites to buy and sell goods. Prices are
    /// influenced by supply levels, approximating the dynamic behaviour of the
    /// original Rust version albeit in a much simpler form.
    /// </summary>
    [Serializable]
    public class Market
    {
        private readonly Dictionary<Good, float> _basePrices = new();
        private readonly Dictionary<Good, float> _stock = new();

        /// <summary>
        /// Desired stock level used when calculating price adjustments.
        /// </summary>
        public float TargetStock { get; set; } = 10f;

        /// <summary>Set the price of <paramref name="good"/>.</summary>
        public void SetPrice(Good good, float price) => _basePrices[good] = price;

        /// <summary>Retrieve the price of <paramref name="good"/> or 1 if unset.</summary>
        public float GetPrice(Good good)
        {
            float basePrice = _basePrices.TryGetValue(good, out var p) ? p : 1f;
            _stock.TryGetValue(good, out var stock);
            float factor = 1f + (TargetStock - stock) / TargetStock;
            factor = Clamp(factor, 0.5f, 2f);
            return basePrice * factor;
        }

        /// <summary>
        /// Attempt to buy <paramref name="amount"/> of <paramref name="good"/> from this market.
        /// Returns <c>true</c> if the buyer had enough coin.
        /// </summary>
        public bool Buy(EconomyData buyer, Good good, float amount)
        {
            float cost = GetPrice(good) * amount;
            if (buyer.Coin < cost || amount <= 0f)
                return false;
            buyer.Coin -= cost;
            buyer.Produce(good, amount);
            _stock.TryGetValue(good, out var stock);
            _stock[good] = Clamp(stock - amount, 0f, float.MaxValue);
            return true;
        }

        /// <summary>
        /// Sell <paramref name="amount"/> of <paramref name="good"/> to this market.
        /// </summary>
        public bool Sell(EconomyData seller, Good good, float amount)
        {
            if (!seller.Consume(good, amount) || amount <= 0f)
                return false;
            float value = GetPrice(good) * amount;
            seller.Coin += value;
            _stock.TryGetValue(good, out var stock);
            _stock[good] = stock + amount;
            return true;
        }

        /// <summary>
        /// Very small decay of stored stock to mimic consumption and let prices
        /// drift back towards their base value.
        /// </summary>
        public void Tick(float dt)
        {
            foreach (var key in _stock.Keys.ToArray())
            {
                float val = _stock[key] - dt;
                _stock[key] = val <= 0f ? 0f : val;
            }
        }

        private static float Clamp(float v, float min, float max)
            => v < min ? min : (v > max ? max : v);
    }
}
