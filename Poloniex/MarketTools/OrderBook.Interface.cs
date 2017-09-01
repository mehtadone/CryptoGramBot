using System.Collections.Generic;

namespace Jojatekok.PoloniexAPI.MarketTools
{
    public interface IOrderBook
    {
        IList<IOrder> BuyOrders { get; }
        IList<IOrder> SellOrders { get; }
    }
}
