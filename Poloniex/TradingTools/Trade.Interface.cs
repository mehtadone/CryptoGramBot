using System;

namespace Jojatekok.PoloniexAPI.TradingTools
{
    public interface ITrade : IOrder
    {
        ulong GlobalTradeId { get; set; }
        string Pair { get; set; }
        DateTime Time { get; }
    }
}