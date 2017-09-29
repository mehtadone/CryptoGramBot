using System;
using Poloniex.MarketTools;

namespace Poloniex.TradingTools
{
    public interface ITrade : IOrder
    {
        string Pair { get; set; }
        DateTime Time { get; }
    }
}