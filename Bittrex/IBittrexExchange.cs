using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bittrex
{
    public interface IExchange
    {
        decimal CalculateMinimumOrderQuantity(string market, decimal price);

        void CancelOrder(string uuid);

        AccountBalance GetBalance(string market);

        GetBalancesResponse GetBalances();

        /// <summary>
        /// Used to retrieve the latest trades that have occured for a specific market.
        /// </summary>
        /// <param name="market"></param>
        /// <param name="count">a number between 1-50 for the number of entries to return</param>
        /// <returns></returns>
        GetMarketHistoryResponse GetMarketHistory(string market, int count = 20);

        dynamic GetMarkets();

        GetMarketSummaryResponse GetMarketSummary(string market);

        GetOpenOrdersResponse GetOpenOrders(string market);

        /// <summary>
        /// Used to retrieve the orderbook for a given market
        /// </summary>
        /// <param name="market"></param>
        /// <param name="type">The type of orderbook to return.</param>
        /// <param name="depth">How deep of an order book to retrieve. Max is 50</param>
        /// <returns></returns>
        GetOrderBookResponse GetOrderBook(string market, OrderBookType type, int depth = 20);

        GetOrderHistoryResponse GetOrderHistory();

        GetOrderHistoryResponse GetOrderHistory(string market, int count = 10);

        dynamic GetTicker(string market);

        void Initialise(ExchangeContext context);

        OrderResponse PlaceBuyOrder(string market, decimal quantity, decimal price);

        OrderResponse PlaceSellOrder(string market, decimal quantity, decimal price);
    }
}