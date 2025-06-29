using System;

namespace VelorenPort.World.Site.Economy
{
    /// <summary>
    /// Very small representation of a caravan traveling between two sites.
    /// On arrival it transfers a fixed amount of a single good from the origin
    /// to the destination and then swaps direction.
    /// </summary>
    [Serializable]
    public class CaravanRoute
    {
        public Site From { get; private set; }
        public Site To { get; private set; }
        public Good Good { get; }
        public float Amount { get; }
        public float TravelTime { get; }
        private float _progress;

        public CaravanRoute(Site from, Site to, Good good, float amount, float travelTime = 1f)
        {
            From = from;
            To = to;
            Good = good;
            Amount = amount;
            TravelTime = travelTime;
            _progress = 0f;
        }

        public void Tick(float dt)
        {
            _progress += dt;
            if (_progress < TravelTime)
                return;
            _progress -= TravelTime;

            // Attempt trade: origin sells to destination via markets
            if (From.Economy.Market.Sell(From.Economy, Good, Amount))
            {
                To.Economy.Market.Buy(To.Economy, Good, Amount);
            }

            // Swap direction for round trip
            (From, To) = (To, From);
        }
    }
}
