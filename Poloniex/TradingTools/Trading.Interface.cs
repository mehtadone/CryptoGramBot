using Jojatekok.PoloniexAPI.TradingTools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jojatekok.PoloniexAPI
{
    public interface ITrading
    {
        /// <summary>
        ///     <para>Cancels an open order identified by the order ID.</para>
        ///     <para>Warning: Order cancellations are processed FIFO (First In First Out) alongside new orders, so it may be matched before the cancellation can be processed.</para>
        /// </summary>
        /// <param name="currencyPair">The currency pair, which consists of the currency being traded on the market, and the base's code.</param>
        /// <param name="orderId">The ID of the order to cancel.</param>
        Task<bool> DeleteOrderAsync(CurrencyPair currencyPair, ulong orderId);

        /// <summary>Fetches the current open orders in your account, ordered by most recent first.</summary>
        /// <param name="currencyPair">The currency pair, which consists of the currency being traded on the market, and the base's code.</param>
        Task<IList<IOrder>> GetOpenOrdersAsync(CurrencyPair currencyPair);

        /// <summary>Fetches the trades made in your account, ordered by most recent first.</summary>
        /// <param name="currencyPair">The currency pair, which consists of the currency being traded on the market, and the base's code.</param>
        Task<IList<ITrade>> GetTradesAsync(CurrencyPair currencyPair);

        /// <summary>Fetches the trades made in your account in a given time period, ordered by most recent first.</summary>
        /// <param name="currencyPair">The currency pair, which consists of the currency being traded on the market, and the base's code.</param>
        /// <param name="startTime">The time to start fetching data from.</param>
        /// <param name="endTime">The time to stop fetching data at.</param>
        Task<IList<ITrade>> GetTradesAsync(CurrencyPair currencyPair, DateTime startTime, DateTime endTime);

        /// <summary>Fetches the trades made in your account in a given time period, ordered by most recent first.</summary>
        /// <param name="currencyPair">The currency pair, which consists of the currency being traded on the market, and the base's code.</param>
        /// <param name="startTime">The time to start fetching data from.</param>
        Task<IList<ITrade>> GetTradesAsync(CurrencyPair currencyPair, DateTime startTime);

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