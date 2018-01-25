using Binance;
using Binance.Account;
using Binance.Account.Orders;
using Binance.Api;
using Binance.Api.WebSocket.Events;
using Binance.Market;
using CryptoGramBot.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoGramBot.Services.Exchanges.WebSockets.Binance
{
    public class BinanceWebsocketService : IBinanceWebsocketService, IDisposable
    {
        #region Fields

        private bool isDisposed;

        #endregion

        #region Dependecies

        private readonly BinanceConfig _config;
        private readonly IBinanceApi _binanceApi;
        private readonly IBinanceCacheService _cache;
        private readonly IBinanceSubscriberService _subscriber;

        #endregion

        #region Constructor

        public BinanceWebsocketService(BinanceConfig config,
           IBinanceApi binanceApi,
           IBinanceCacheService cache,
           IBinanceSubscriberService subscriber)
        {
            _config = config;
            _binanceApi = binanceApi;
            _cache = cache;
            _subscriber = subscriber;
        } 

        #endregion

        #region IBinanceWebsocketService

        public Task<AccountInfo> GetAccountInfoAsync()
        {
            return GetAccountInfo();
        }

        public async Task<IEnumerable<Order>> GetOpenOrdersAsync(string symbol)
        {
            return await GetOrders(symbol);
        }

        public async Task<IEnumerable<AccountTrade>> GetAccountTradesAsync(string symbol)
        {
            return await GetAccountTrades(symbol);
        }

        public async Task<IEnumerable<Symbol>> GetSymbolsAsync()
        {
            return await GetSymbols();
        }

        public async Task<SymbolPrice> GetPriceAsync(string symbol)
        {
            var symbolPrice = await GetSymbolPrice(symbol);

            return symbolPrice == null ? null : new SymbolPrice(symbol, symbolPrice.Value);
        }

        public async Task<IEnumerable<SymbolPrice>> GetPricesAsync()
        {
            return await GetSymbolPrices();
        }

        public async Task<IEnumerable<Candlestick>> GetCandlestickAsync(string symbol, CandlestickInterval interval)
        {
            return await GetCandlestick(symbol, interval);
        }

        public async Task<IEnumerable<SymbolStatistics>> Get24HourStatisticsAsync()
        {
            return await GetSymbolStatistics();
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (!isDisposed)
            {
                _subscriber.Dispose();

                isDisposed = true;
            }
        }

        #endregion

        #region Private methods

        #region Wrappers

        private Task<AccountInfo> GetAccountInfo()
        {
            return GetOrCreateObject(
                () => _cache.GetAccountInfo(),
                () => InitializeAccountInfo(),
                () => SubscribeUserData());
        }

        private Task<ImmutableList<Order>> GetOrders(string symbol)
        {
            return GetOrCreateObject(
                () => _cache.GetOrders(symbol),
                () => InitializeOpenOrders(symbol),
                () => SubscribeUserData());
        }

        private Task<ImmutableList<AccountTrade>> GetAccountTrades(string symbol)
        {
            return GetOrCreateObject(
                () => _cache.GetAccountTrades(symbol),
                () => InitializeAccountTrades(symbol),
                () => SubscribeUserData());
        }

        private Task<ImmutableList<Candlestick>> GetCandlestick(string symbol, CandlestickInterval interval)
        {
            return GetOrCreateObject(
                () => _cache.GetCandlesticks(symbol, interval),
                () => InitializeCandleticks(symbol, interval),
                () => SubscribeCandlestick(symbol, interval));
        }

        private Task<List<Symbol>> GetSymbols()
        {
            return GetOrCreateObject(
                () => _cache.GetSymbols(),
                () => InitializeSymbols());
        }

        private async Task<decimal?> GetSymbolPrice(string symbol)
        {
            var symbolPrices = await GetOrCreateObject(
                   () => _cache.GetSymbolPrices(),
                   () => InitializeSymbolPrices(),
                   () => SubscribeSymbols());

            if (symbolPrices.ContainsKey(symbol))
            {
                return symbolPrices[symbol];
            }

            return null;
        }

        private async Task<List<SymbolPrice>> GetSymbolPrices()
        {
            var dictionary = await GetOrCreateObject(
                () => _cache.GetSymbolPrices(),
                () => InitializeSymbolPrices(),
                () => SubscribeSymbols());

            return dictionary.Select(p => new SymbolPrice(p.Key, p.Value)).ToList();
        }

        private async Task<List<SymbolStatistics>> GetSymbolStatistics()
        {
            var dictionary = await GetOrCreateObject(
                () => _cache.GetSymbolStatistics(),
                () => InitializeSymbolStatistics(),
                () => SubscribeSymbols());

            return dictionary.Select(p => p.Value).ToList();
        }

        private void SubscribeSymbols()
        {
            _subscriber.SymbolsStatistics(OnStatisticsUpdate);
        }

        private void SubscribeUserData()
        {
            _subscriber.UserData(OnOrderUpdate, OnAccountUpdate, OnAccountTradeUpdate);
        }

        private void SubscribeCandlestick(string symbol, CandlestickInterval interval)
        {
            _subscriber.Candlestick(symbol, interval, OnCandletickUpdate);
        }

        #endregion

        #region Initial initialization

        private async Task<T> GetOrCreateObject<T>(Func<T> getFromCache,
            Func<Task> initialize,
            Action subscribeTo = null)
            where T : class
        {
            if (getFromCache() == default(T))
            {
                await initialize();
            }

            subscribeTo?.Invoke();

            return getFromCache();
        }

        private async Task InitializeAccountInfo()
        {
            using (var user = new BinanceApiUser(_config.Key, _config.Secret))
            {
                var accountInfo = await _binanceApi.GetAccountInfoAsync(user);

                _cache.SetAccountInfo(accountInfo);
            }
        }

        private async Task InitializeOpenOrders(string symbol)
        {
            using (var user = new BinanceApiUser(_config.Key, _config.Secret))
            {
                var openOrders = await _binanceApi.GetOpenOrdersAsync(user, symbol);

                _cache.SetOrders(symbol, openOrders.ToImmutableList());
            }
        }

        private async Task InitializeAccountTrades(string symbol)
        {
            using (var user = new BinanceApiUser(_config.Key, _config.Secret))
            {
                var accountTrades = await _binanceApi.GetAccountTradesAsync(user, symbol);

                _cache.SetAccountTrades(symbol, accountTrades.ToImmutableList());
            }
        }

        private async Task InitializeCandleticks(string symbol, CandlestickInterval interval)
        {
            var candlesticks = await _binanceApi.GetCandlesticksAsync(symbol, interval);

            _cache.SetCandlestick(symbol, interval, candlesticks.ToImmutableList());
        }

        private async Task InitializeSymbols()
        {
            var symbols = await _binanceApi.GetSymbolsAsync();

            _cache.SetSymbols(symbols.ToList());
        }

        private async Task InitializeSymbolPrices()
        {
            var symbolPrices = await _binanceApi.GetPricesAsync();

            _cache.SetSymbolPrices(symbolPrices.ToImmutableDictionary(s => s.Symbol, s => s.Value));
        }

        private async Task InitializeSymbolStatistics()
        {
            var symbolStatistics = await _binanceApi.Get24HourStatisticsAsync();

            _cache.SetSymbolStatistics(symbolStatistics.ToImmutableDictionary(s => s.Symbol, s => s));
        }

        #endregion

        #region Event Handlers Methods

        private void OnAccountTradeUpdate(AccountTradeUpdateEventArgs e)
        {
            var symbol = e.Trade.Symbol;
            var immutableAccountTrades = _cache.GetAccountTrades(symbol);

            if (immutableAccountTrades != null)
            {
                var mutableTrades = immutableAccountTrades.ToBuilder();

                var previousTrade = mutableTrades.FirstOrDefault(p => p.Id == e.Trade.Id);

                if (previousTrade != null)
                {
                    mutableTrades.Remove(previousTrade);
                }

                mutableTrades.Add(e.Trade);

                _cache.SetAccountTrades(symbol, mutableTrades.ToImmutable());

                UpdateOrders(e.Order, symbol);
            }
        }

        private void OnAccountUpdate(AccountUpdateEventArgs e)
        {
            _cache.SetAccountInfo(e.AccountInfo);
        }

        private void OnOrderUpdate(OrderUpdateEventArgs args)
        {
            var symbol = args.Order.Symbol;

            UpdateOrders(args.Order, symbol);
        }

        private void UpdateOrders(Order updatedOrder, string symbol)
        {
            var immutableOrders = _cache.GetOrders(symbol);

            if (immutableOrders != null)
            {
                var mutableOrders = immutableOrders.ToBuilder();

                var previousOrder = mutableOrders.FirstOrDefault(p => updatedOrder.Id == p.Id);

                if (previousOrder != null)
                {
                    mutableOrders.Remove(previousOrder);
                }

                mutableOrders.Add(updatedOrder);

                _cache.SetOrders(symbol, mutableOrders.ToImmutable());
            }
        }

        private void OnStatisticsUpdate(SymbolStatisticsEventArgs e)
        {
            var statistics = e.Statistics;

            var immutableStatistics = _cache.GetSymbolStatistics();
            var immutablePrices = _cache.GetSymbolPrices();

            if (immutableStatistics != null)
            {
                var mutableStatistics = immutableStatistics.ToBuilder();

                foreach (var statistic in statistics)
                {
                    mutableStatistics[statistic.Symbol] = statistic;
                }

                _cache.SetSymbolStatistics(mutableStatistics.ToImmutable());
            }

            if (immutablePrices != null)
            {
                var mutablePrice = immutablePrices.ToBuilder();

                foreach (var statistic in statistics)
                {
                    mutablePrice[statistic.Symbol] = statistic.LastPrice;
                }

                _cache.SetSymbolPrices(mutablePrice.ToImmutable());
            }
        }

        private void OnCandletickUpdate(CandlestickEventArgs args)
        {
            var immutableCandleticks = _cache.GetCandlesticks(args.Candlestick.Symbol, args.Candlestick.Interval);

            if(immutableCandleticks != null && immutableCandleticks.Any())
            {
                var mutableCandletickes = immutableCandleticks.ToBuilder();

                var previousCandletick = mutableCandletickes.FirstOrDefault(p => p.OpenTime == args.Candlestick.OpenTime);

                mutableCandletickes.Remove(previousCandletick ?? mutableCandletickes.FirstOrDefault());

                mutableCandletickes.Add(args.Candlestick);

                _cache.SetCandlestick(args.Candlestick.Symbol, args.Candlestick.Interval, mutableCandletickes.ToImmutable());
            }
        }

        #endregion

        #endregion
    }
}
