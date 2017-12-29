using Jojatekok.PoloniexAPI.MarketTools;
using System;

namespace Jojatekok.PoloniexAPI
{
    [Serializable]
    public class TickerChangedEventArgs : EventArgs
    {
        public CurrencyPair CurrencyPair { get; private set; }
        public MarketData MarketData { get; private set; }

        public TickerChangedEventArgs(CurrencyPair currencyPair, MarketData marketData)
        {
            CurrencyPair = currencyPair;
            MarketData = marketData;
        }
    }
}
