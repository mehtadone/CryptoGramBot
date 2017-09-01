using Newtonsoft.Json;

namespace Jojatekok.PoloniexAPI.WalletTools
{
    public class Balance : IBalance
    {
        [JsonProperty("available")]
        public double QuoteAvailable { get; private set; }
        [JsonProperty("onOrders")]
        public double QuoteOnOrders { get; private set; }
        [JsonProperty("btcValue")]
        public double BitcoinValue { get; private set; }
    }
}
