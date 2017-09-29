using System;
using Poloniex.General;
using Poloniex.MarketTools;

namespace Jojatekok.PoloniexAPI
{
    public class TickerChangedEventArgs : EventArgs
    {
        internal TickerChangedEventArgs(CurrencyPair currencyPair, MarketData marketData)
        {
            CurrencyPair = currencyPair;
            MarketData = marketData;
        }

        public CurrencyPair CurrencyPair { get; private set; }
        public MarketData MarketData { get; private set; }
    }
}