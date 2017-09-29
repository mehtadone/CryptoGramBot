using Newtonsoft.Json;
using Poloniex.General;

namespace Poloniex.TradingTools
{
    public class Order : IOrder
    {
        [JsonProperty("total")]
        public double AmountBase { get; private set; }

        [JsonProperty("amount")]
        public double AmountQuote { get; private set; }

        [JsonProperty("orderNumber")]
        public ulong IdOrder { get; private set; }

        [JsonProperty("rate")]
        public double PricePerCoin { get; private set; }

        public OrderType Type { get; private set; }

        [JsonProperty("type")]
        private string TypeInternal
        {
            set { Type = value.ToOrderType(); }
        }
    }
}