using System;
using System.Collections.Generic;
using VelorenPort.CoreEngine;

namespace VelorenPort.World.Civ
{
    /// <summary>
    /// Minimal economic helper types translated from <c>econ.rs</c>.
    /// </summary>
    public class SellOrder
    {
        public float Quantity { get; set; }
        public float Price { get; set; }
        public float QuantitySold { get; set; }
    }

    public class BuyOrder
    {
        public float Quantity { get; set; }
        public float MaxPrice { get; set; }
    }

    public class Belief
    {
        public float Price { get; set; }
        public float Confidence { get; set; }

        public float ChoosePrice(Random rng)
        {
            return Price + ((float)rng.NextDouble() * 2f - 1f) * Confidence;
        }

        public void UpdateBuyer(float years, float newPrice)
        {
            if (MathF.Abs(Price - newPrice) < Confidence)
            {
                Confidence *= 0.8f;
            }
            else
            {
                Price += (newPrice - Price) * 0.5f;
                Confidence = MathF.Abs(Price - newPrice);
            }
        }

        public void UpdateSeller(float proportion)
        {
            Price *= 1f + (proportion - 0.5f) * 0.25f;
            Confidence /= 1f + (proportion - 0.5f) * 0.25f;
        }
    }

    public static class Econ
    {
        /// <summary>
        /// Purchase units from multiple sell orders up to the given limits.
        /// Returns the quantity bought and the amount spent.
        /// </summary>
        public static (float quantity, float spent) BuyUnits(Random rng, IEnumerable<SellOrder> sellers, float maxQuantity, float maxPrice, float maxSpend)
        {
            var orders = new List<SellOrder>();
            foreach (var s in sellers)
                if (s.Quantity > 0f) orders.Add(s);
            orders.Sort((a, b) => a.Price.CompareTo(b.Price));

            float q = 0f;
            float spent = 0f;
            foreach (var order in orders)
            {
                if (q >= maxQuantity || spent >= maxSpend || order.Price > maxPrice)
                    break;
                float take = MathF.Min(MathF.Min(maxQuantity - q, order.Quantity - order.QuantitySold), (maxSpend - spent) / order.Price);
                order.QuantitySold += take;
                q += take;
                spent += take * order.Price;
            }
            return (q, spent);
        }
    }
}
