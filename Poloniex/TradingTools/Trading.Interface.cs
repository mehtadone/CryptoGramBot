using Jojatekok.PoloniexAPI.TradingTools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jojatekok.PoloniexAPI.MarketTools;
using IOrder = Jojatekok.PoloniexAPI.TradingTools.IOrder;
using ITrade = Jojatekok.PoloniexAPI.TradingTools.ITrade;
using Order = Jojatekok.PoloniexAPI.TradingTools.Order;

namespace Jojatekok.PoloniexAPI
{
    /// <summary>
    /// The <see cref="ITrading"/> interface defines methods used to interact with Poloniex&apos;s trading service.
    /// </summary>
    public interface ITrading
    {
        /// <summary>Fetches information about your current margin position.</summary>
        /// <param name="currencyPair">The currency pair, which consists of the currency being traded on the market, and the base's code.</param>
        /// <returns>A <see cref="IPosition"/> instance that describes the active position, if any.</returns>
        Task<IList<ITrade>> CloseMarginPositionAsync(CurrencyPair currencyPair);

        /// <summary>
        ///     <para>Cancels an open order identified by the order ID.</para>
        ///     <para>Warning: Order cancellations are processed FIFO (First In First Out) alongside new orders, so it may be matched before the cancellation can be processed.</para>
        /// </summary>
        /// <param name="currencyPair">The currency pair, which consists of the currency being traded on the market, and the base's code.</param>
        /// <param name="orderId">The ID of the order to cancel.</param>
        Task<bool> DeleteOrderAsync(CurrencyPair currencyPair, ulong orderId);

        /// <summary>Fetches information about your current margin position.</summary>
        /// <returns>A <see cref="IPosition"/> instance that describes the active position, if any.</returns>
        Task<IDictionary<CurrencyPair, IPosition>> GetAllMarginPositionsAsync();

        /// <summary>Fetches information about your current margin position.</summary>
        /// <returns>A <see cref="IMarginAccountSummary"/> instance that describes the active position, if any.</returns>
        Task<IMarginAccountSummary> GetMarginAccountSummaryAsync();

        /// <summary>Fetches information about your current margin position.</summary>
        /// <param name="currencyPair">The currency pair, which consists of the currency being traded on the market, and the base's code.</param>
        /// <returns>A <see cref="IPosition"/> instance that describes the active position, if any.</returns>
        Task<IPosition> GetMarginPositionAsync(CurrencyPair currencyPair);

        /// <summary>Fetches the current open orders in your account, ordered by most recent first.</summary>
        /// <param name="currencyPair">The currency pair, which consists of the currency being traded on the market, and the base's code.</param>
        Task<IList<IOrder>> GetOpenOrdersAsync(CurrencyPair currencyPair);

        Task<Dictionary<string, List<Order>>> GetOpenOrdersAsync();

        /// <summary>Fetches current tradeable balances.</summary>
        /// <returns>A <see cref="IPosition"/> instance that describes the active position, if any.</returns>
        Task<IDictionary<CurrencyPair, ITradeableBalance>> GetTradeableBalancesAsync();

        /// <summary>Fetches the trades made in your account, ordered by most recent first.</summary>
        /// <param name="currencyPair">The currency pair, which consists of the currency being traded on the market, and the base's code.</param>
        Task<IList<ITrade>> GetTradesAsync(CurrencyPair currencyPair);

        /// <summary>Fetches the trades made in your account in a given time period, ordered by most recent first.</summary>
        /// <param name="currencyPair">The currency pair, which consists of the currency being traded on the market, and the base's code.</param>
        /// <param name="startTime">The time to start fetching data from.</param>
        /// <param name="endTime">The time to stop fetching data at.</param>
        Task<IList<ITrade>> GetTradesAsync(CurrencyPair currencyPair, DateTime startTime, DateTime endTime);

        /// <summary>Cancels an order and places a new one of the same type in a single atomic transaction, meaning either both operations will succeed or both will fail.</summary>
        /// <param name="orderId">The ID of the order to cancel.</param>
        /// <param name="newRate">The new rate for the order</param>
        /// <param name="amount">Optional change in amount.</param>
        Task<string> MoveOrderAsync(ulong orderId, double newRate, double? amount = default(double?));

        /// <summary>Submits a new order to the market.</summary>
        /// <param name="currencyPair">The currency pair, which consists of the currency being traded on the market, and the base's code.</param>
        /// <param name="type">Type of the order.</param>
        /// <param name="pricePerCoin">The price to trade your coins at, compared to the base currency.</param>
        /// <param name="amountQuote">The amount of quote you want to trade.</param>
        Task<string> PostMarginOrderAsync(CurrencyPair currencyPair, OrderType type, double pricePerCoin, double amountQuote, double lendingRate);

        /// <summary>Submits a new order to the market.</summary>
        /// <param name="currencyPair">The currency pair, which consists of the currency being traded on the market, and the base's code.</param>
        /// <param name="type">Type of the order.</param>
        /// <param name="pricePerCoin">The price to trade your coins at, compared to the base currency.</param>
        /// <param name="amountQuote">The amount of quote you want to trade.</param>
        Task<string> PostOrderAsync(CurrencyPair currencyPair, OrderType type, double pricePerCoin, double amountQuote);
    }
}