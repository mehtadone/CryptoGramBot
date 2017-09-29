using Jojatekok.PoloniexAPI;
using Newtonsoft.Json;

namespace Poloniex.MarketTools
{
    public class MarketData : IMarketData
    {
        public bool IsFrozen { get; private set; }

        public double OrderSpread
        {
            get { return (OrderTopSell - OrderTopBuy).Normalize(); }
        }

        public double OrderSpreadPercentage
        {
            get { return OrderTopSell / OrderTopBuy - 1; }
        }

        [JsonProperty("highestBid")]
        public double OrderTopBuy { get; internal set; }

        [JsonProperty("lowestAsk")]
        public double OrderTopSell { get; internal set; }

        [JsonProperty("percentChange")]
        public double PriceChangePercentage { get; internal set; }

        [JsonProperty("last")]
        public double PriceLast { get; internal set; }

        [JsonProperty("baseVolume")]
        public double Volume24HourBase { get; internal set; }

        [JsonProperty("quoteVolume")]
        public double Volume24HourQuote { get; internal set; }

        [JsonProperty("isFrozen")]
        internal byte IsFrozenInternal
        {
            set { IsFrozen = value != 0; }
        }
    }
}