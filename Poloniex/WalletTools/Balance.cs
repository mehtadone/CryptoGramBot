using Newtonsoft.Json;

namespace Poloniex.WalletTools
{
    public class Balance : IBalance
    {
        [JsonProperty("btcValue")]
        public double BitcoinValue { get; private set; }

        [JsonProperty("available")]
        public double QuoteAvailable { get; private set; }

        [JsonProperty("onOrders")]
        public double QuoteOnOrders { get; private set; }
    }
}