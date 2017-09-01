using Jojatekok.PoloniexAPI.MarketTools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jojatekok.PoloniexAPI
{
    public interface IMarkets
    {
        /// <summary>Gets a data summary of the markets available.</summary>
        Task<IDictionary<CurrencyPair, IMarketData>> GetSummaryAsync();

        /// <summary>Fetches the best priced orders for a given market.</summary>
        /// <param name="currencyPair">The currency pair, which consists of the currency being traded on the market, and the base's code.</param>
        /// <param name="depth">The number of orders to fetch from each side.</param>
        Task<IOrderBook> GetOpenOrdersAsync(CurrencyPair currencyPair, uint depth = 50);

        /// <summary>Fetches the last 200 trades of a given market.</summary>
        /// <param name="currencyPair">The currency pair, which consists of the currency being traded on the market, and the base's code.</param>
        Task<IList<ITrade>> GetTradesAsync(CurrencyPair currencyPair);

        /// <summary>Fetches the trades of a given market in a given time period.</summary>
        /// <param name="currencyPair">The currency pair, which consists of the currency being traded on the market, and the base's code.</param>
        /// <param name="startTime">The time to start fetching data from.</param>
        /// <param name="endTime">The time to stop fetching data at.</param>
        Task<IList<ITrade>> GetTradesAsync(CurrencyPair currencyPair, DateTime startTime, DateTime endTime);

        /// <summary>Fetches the chart data which Poloniex uses for their candlestick graphs for a market view of a given time period.</summary>
        /// <param name="currencyPair">The currency pair, which consists of the currency being traded on the market, and the base's code.</param>
        /// <param name="period">The sampling frequency of the chart.</param>
        /// <param name="startTime">The time to start fetching data from.</param>
        /// <param name="endTime">The time to stop fetching data at.</param>
        Task<IList<IMarketChartData>> GetChartDataAsync(CurrencyPair currencyPair, MarketPeriod period, DateTime startTime, DateTime endTime);

        /// <summary>Fetches the chart data which Poloniex uses for their candlestick graphs for a market view of a given time period.</summary>
        /// <param name="currencyPair">The currency pair, which consists of the currency being traded on the market, and the base's code.</param>
        /// <param name="period">The sampling frequency of the chart.</param>
        Task<IList<IMarketChartData>> GetChartDataAsync(CurrencyPair currencyPair, MarketPeriod period);

        /// <summary>Fetches the chart data which Poloniex uses for their candlestick graphs for a market view with a period of 30 minutes.</summary>
        /// <param name="currencyPair">The currency pair, which consists of the currency being traded on the market, and the base's code.</param>
        Task<IList<IMarketChartData>> GetChartDataAsync(CurrencyPair currencyPair);
    }
}
