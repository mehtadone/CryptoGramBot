using System;

namespace Jojatekok.PoloniexAPI.TradingTools
{
    public interface ITrade : IOrder
    {
        string Pair { get; }
        DateTime Time { get; }
    }
}