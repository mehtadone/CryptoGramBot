using System;
using Jojatekok.PoloniexAPI;
using Newtonsoft.Json;

namespace Poloniex.MarketTools
{
    public class MarketChartData : IMarketChartData
    {
        [JsonProperty("close")]
        public double Close { get; private set; }

        [JsonProperty("high")]
        public double High { get; private set; }

        [JsonProperty("low")]
        public double Low { get; private set; }

        [JsonProperty("open")]
        public double Open { get; private set; }

        public DateTime Time { get; private set; }

        [JsonProperty("volume")]
        public double VolumeBase { get; private set; }

        [JsonProperty("quoteVolume")]
        public double VolumeQuote { get; private set; }

        [JsonProperty("weightedAverage")]
        public double WeightedAverage { get; private set; }

        [JsonProperty("date")]
        private ulong TimeInternal
        {
            set { Time = Helper.UnixTimeStampToDateTime(value); }
        }
    }
}