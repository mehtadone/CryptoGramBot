using System.Threading.Tasks;
using Bittrex.Data;

namespace Bittrex
{
    public interface IExchange
    {
        decimal CalculateMinimumOrderQuantity(string market, decimal price);

        Task CancelOrder(string uuid);

        Task<AccountBalance> GetBalance(string market);

        Task<GetBalancesResponse> GetBalances();

        Task<GetDepositResponse> GetDeposits();

        /// <summary>
        /// Used to retrieve the latest trades that have occured for a specific market.
        /// </summary>
        /// <param name="market"></param>
        /// <param name="count">a number between 1-50 for the number of entries to return</param>
        /// <returns></returns>
        Task<GetMarketHistoryResponse> GetMarketHistory(string market, int count = 20);

        dynamic GetMarkets();

        Task<GetMarketSummaryResponse> GetMarketSummary(string market);

        Task<GetOpenOrdersResponse> GetOpenOrders(string market);

        /// <summary>
        /// Used to retrieve the orderbook for a given market
        /// </summary>
        /// <param name="market"></param>
        /// <param name="type">The type of orderbook to return.</param>
        /// <param name="depth">How deep of an order book to retrieve. Max is 50</param>
        /// <returns></returns>
        Task<GetOrderBookResponse> GetOrderBook(string market, OrderBookType type, int depth = 20);

        Task<GetOrderHistoryResponse> GetOrderHistory();

        Task<GetOrderHistoryResponse> GetOrderHistory(string market, int count = 10);

        dynamic GetTicker(string market);

        Task<GetWithdrawalResponse> GetWithdrawals();

        void Initialise(ExchangeContext context);

        Task<OrderResponse> PlaceBuyOrder(string market, decimal quantity, decimal price);

        Task<OrderResponse> PlaceSellOrder(string market, decimal quantity, decimal price);
    }
}