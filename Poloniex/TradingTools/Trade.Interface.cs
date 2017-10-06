using System;
using Poloniex.MarketTools;

namespace Poloniex.TradingTools
{
    public interface ITrade : IOrder
    {
        ulong GlobalTradeId { get; set; }
        string Pair { get; set; }
        DateTime Time { get; }
    }
}