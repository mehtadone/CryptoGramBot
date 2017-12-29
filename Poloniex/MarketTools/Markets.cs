using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Jojatekok.PoloniexAPI.MarketTools
{
    public class Markets : IMarkets
    {
        private ApiWebClient ApiWebClient { get; set; }

        internal Markets(ApiWebClient apiWebClient)
        {
            ApiWebClient = apiWebClient;
        }

        private IDictionary<CurrencyPair, IMarketData> GetSummary()
        {
            var data = GetData<IDictionary<string, MarketData>>("returnTicker");
            return data.ToDictionary(
                x => CurrencyPair.Parse(x.Key),
                x => (IMarketData)x.Value
            );
        }

        private IOrderBook GetOpenOrders(CurrencyPair currencyPair, uint depth)
        {
            var data = GetData<OrderBook>(
                "returnOrderBook",
                "currencyPair=" + currencyPair,
                "depth=" + depth
            );
            return data;
        }

        private IDictionary<CurrencyPair, IOrderBook> GetAllOpenOrders(uint depth)
        {
            var data = GetData<IDictionary<string, OrderBook>>(
                "returnOrderBook",
                "currencyPair=all",
                "depth=" + depth
            );
            return data.ToDictionary(
                x => CurrencyPair.Parse(x.Key),
                x => (IOrderBook)x.Value
            );
        }

        private IList<ITrade> GetTrades(CurrencyPair currencyPair)
        {
            var data = GetData<IList<Trade>>(
                "returnTradeHistory",
                "currencyPair=" + currencyPair
            );
            return new List<ITrade>(data);
        }

        private IList<ITrade> GetTrades(CurrencyPair currencyPair, DateTime startTime, DateTime endTime)
        {
            var data = GetData<IList<Trade>>(
                "returnTradeHistory",
                "currencyPair=" + currencyPair,
                "start=" + Helper.DateTimeToUnixTimeStamp(startTime),
                "end=" + Helper.DateTimeToUnixTimeStamp(endTime)
            );
            return new List<ITrade>(data);
        }

        private IList<IMarketChartData> GetChartData(CurrencyPair currencyPair, MarketPeriod period, DateTime startTime, DateTime endTime)
        {
            var data = GetData<IList<MarketChartData>>(
                "returnChartData",
                "currencyPair=" + currencyPair,
                "start=" + Helper.DateTimeToUnixTimeStamp(startTime),
                "end=" + Helper.DateTimeToUnixTimeStamp(endTime),
                "period=" + (int)period
            );
            return new List<IMarketChartData>(data);
        }

        /// <inheritdoc cref="IMarkets.GetSummaryAsync"/>
        public Task<IDictionary<CurrencyPair, IMarketData>> GetSummaryAsync()
        {
            return Task.Factory.StartNew(() => GetSummary());
        }
        
        /// <inheritdoc cref="IMarkets.GetAllOpenOrdersAsync"/>
        public Task<IDictionary<CurrencyPair, IOrderBook>> GetAllOpenOrdersAsync(uint depth = 50)
        {
            return Task.Factory.StartNew(() => GetAllOpenOrders(depth));
        }

        /// <inheritdoc cref="IMarkets.GetOpenOrdersAsync"/>
        public Task<IOrderBook> GetOpenOrdersAsync(CurrencyPair currencyPair, uint depth)
        {
            return Task.Factory.StartNew(() => GetOpenOrders(currencyPair, depth));
        }

        /// <inheritdoc cref="IMarkets.GetTradesAsync(CurrencyPair)"/>
        public Task<IList<ITrade>> GetTradesAsync(CurrencyPair currencyPair)
        {
            return Task.Factory.StartNew(() => GetTrades(currencyPair));
        }

        /// <inheritdoc cref="IMarkets.GetTradesAsync(CurrencyPair, DateTime, DateTime)"/>
        public Task<IList<ITrade>> GetTradesAsync(CurrencyPair currencyPair, DateTime startTime, DateTime endTime)
        {
            return Task.Factory.StartNew(() => GetTrades(currencyPair, startTime, endTime));
        }

        /// <inheritdoc cref="IMarkets.GetChartDataAsync(CurrencyPair, MarketPeriod, DateTime, DateTime)"/>
        public Task<IList<IMarketChartData>> GetChartDataAsync(CurrencyPair currencyPair, MarketPeriod period, DateTime startTime, DateTime endTime)
        {
            return Task.Factory.StartNew(() => GetChartData(currencyPair, period, startTime, endTime));
        }

        /// <inheritdoc cref="IMarkets.GetChartDataAsync(CurrencyPair, MarketPeriod)"/>
        public Task<IList<IMarketChartData>> GetChartDataAsync(CurrencyPair currencyPair, MarketPeriod period)
        {
            return Task.Factory.StartNew(() => GetChartData(currencyPair, period, Helper.DateTimeUnixEpochStart, DateTime.MaxValue));
        }

        /// <inheritdoc cref="IMarkets.GetChartDataAsync(CurrencyPair)"/>
        public Task<IList<IMarketChartData>> GetChartDataAsync(CurrencyPair currencyPair)
        {
            return Task.Factory.StartNew(() => GetChartData(currencyPair, MarketPeriod.Minutes30, Helper.DateTimeUnixEpochStart, DateTime.MaxValue));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T GetData<T>(string command, params object[] parameters)
        {
            return ApiWebClient.GetData<T>(Helper.ApiUrlHttpsRelativePublic + command, parameters);
        }

       
    }
}
