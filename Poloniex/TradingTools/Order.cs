using Newtonsoft.Json;

namespace Jojatekok.PoloniexAPI.TradingTools
{
    public class Order : IOrder
    {
        [JsonProperty("orderNumber")]
        public ulong IdOrder { get; private set; }

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
