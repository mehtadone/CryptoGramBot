using System;

namespace Jojatekok.PoloniexAPI.MarketTools
{
    public interface ITrade
    {
        DateTime Time { get; }

        OrderType Type { get; }

        double PricePerCoin { get; }

        double AmountQuote { get; }
        double AmountBase { get; }
    }
}
