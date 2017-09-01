using Newtonsoft.Json;
using System;

namespace Jojatekok.PoloniexAPI.MarketTools
{
    public class Trade : ITrade
    {
        [JsonProperty("date")]
        private string TimeInternal {
            set { Time = Helper.ParseDateTime(value); }
        }
        public DateTime Time { get; private set; }

        [JsonProperty("type")]
        private string TypeInternal {
            set { Type = value.ToOrderType(); }
        }
        public OrderType Type { get; private set; }

        [JsonProperty("rate")]
        public double PricePerCoin { get; private set; }

        [JsonProperty("amount")]
        public double AmountQuote { get; private set; }
        [JsonProperty("total")]
        public double AmountBase { get; private set; }
    }
}
