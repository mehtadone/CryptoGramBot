using System;
using Jojatekok.PoloniexAPI;
using Newtonsoft.Json;
using Poloniex.General;

namespace Poloniex.MarketTools
{
    public class Trade : ITrade
    {
        [JsonProperty("total")]
        public double AmountBase { get; private set; }

        [JsonProperty("amount")]
        public double AmountQuote { get; private set; }

        [JsonProperty("rate")]
        public double PricePerCoin { get; private set; }

        public DateTime Time { get; private set; }

        public OrderType Type { get; private set; }

        [JsonProperty("date")]
        private string TimeInternal
        {
            set { Time = Helper.ParseDateTime(value); }
        }

        [JsonProperty("type")]
        private string TypeInternal
        {
            set { Type = value.ToOrderType(); }
        }
    }
}