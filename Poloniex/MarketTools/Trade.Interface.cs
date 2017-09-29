using System;
using Poloniex.General;

namespace Poloniex.MarketTools
{
    public interface ITrade
    {
        double AmountBase { get; }
        double AmountQuote { get; }
        double PricePerCoin { get; }
        DateTime Time { get; }

        OrderType Type { get; }
    }
}