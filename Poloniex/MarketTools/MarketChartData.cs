using Newtonsoft.Json;
using System;

namespace Jojatekok.PoloniexAPI.MarketTools
{
    public class MarketChartData  : IMarketChartData
    {
        [JsonProperty("date")]
        private ulong TimeInternal {
            set { Time = Helper.UnixTimeStampToDateTime(value); }
        }
        public DateTime Time { get; private set; }

        [JsonProperty("open")]
        public double Open { get; private set; }
        [JsonProperty("close")]
        public double Close { get; private set; }

        [JsonProperty("high")]
        public double High { get; private set; }
        [JsonProperty("low")]
        public double Low { get; private set; }

        [JsonProperty("volume")]
        public double VolumeBase { get; private set; }
        [JsonProperty("quoteVolume")]
        public double VolumeQuote { get; private set; }

        [JsonProperty("weightedAverage")]
        public double WeightedAverage { get; private set; }
    }
}
