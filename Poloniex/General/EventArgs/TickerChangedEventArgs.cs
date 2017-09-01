using Jojatekok.PoloniexAPI.MarketTools;
using System;

namespace Jojatekok.PoloniexAPI
{
    public class TickerChangedEventArgs : EventArgs
    {
        public CurrencyPair CurrencyPair { get; private set; }
        public MarketData MarketData { get; private set; }

        internal TickerChangedEventArgs(CurrencyPair currencyPair, MarketData marketData)
        {
            CurrencyPair = currencyPair;
            MarketData = marketData;
        }
    }
}
