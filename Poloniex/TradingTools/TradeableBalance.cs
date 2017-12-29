using System.Collections.Generic;
using Jojatekok.PoloniexAPI.MarketTools;
using Newtonsoft.Json;

namespace Jojatekok.PoloniexAPI.TradingTools
{
    public class TradeableBalance
        : ITradeableBalance
    {
        public TradeableBalance()
        {
            
        }
        public TradeableBalance(CurrencyPair currency, IDictionary<string, double> prices)
        {
            Buy = prices[currency.BaseCurrency];
            Sell = prices[currency.QuoteCurrency];
        }

        [JsonProperty("buy")]
        public double Buy { get; private set; }
        [JsonProperty("sell")]
        public double Sell { get; private set; }
    }
}
