using System.Collections.Generic;

namespace Poloniex.MarketTools
{
    public interface IOrderBook
    {
        IList<IOrder> BuyOrders { get; }
        IList<IOrder> SellOrders { get; }
    }
}