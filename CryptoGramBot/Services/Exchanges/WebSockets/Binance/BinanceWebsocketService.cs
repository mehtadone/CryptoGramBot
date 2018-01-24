using Binance.Account;
using Binance.Account.Orders;
using Binance.Api;
using Binance.Api.WebSocket.Events;
using Binance.Market;
using CryptoGramBot.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoGramBot.Services.Exchanges.WebSockets.Binance
{
    public class BinanceWebsocketService : IDisposable
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

        public async Task<SymbolPrice> GetPriceAsync(string symbol)
        {
            var symbolPrice = await GetSymbolPrice(symbol);

            return new SymbolPrice(symbol, symbolPrice);
        }

        public async Task<IEnumerable<SymbolPrice>> GetPricesAsync()
        {
            return await GetSymbolPrices();
        }

        public async Task<IEnumerable<Candlestick>> GetCandlestickAsync(string symbol, CandlestickInterval interval)
        {
            return await GetCandlestick(symbol, interval);
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
            return GetOrCreateUserObject(
                () => _cache.GetAccountInfo(),
                () => InitializeAccountInfo(),
                (accountInfo) => _cache.SetAccountInfo(accountInfo),
                () => SubscribeUserData());
        }

        private Task<ImmutableList<Order>> GetOrders(string symbol)
        {
            return GetOrCreateUserObject(
                () => _cache.GetOrders(symbol),
                () => InitializeOpenOrders(symbol),
                (orders) => _cache.SetOrders(symbol, orders),
                () => SubscribeUserData());
        }

        private Task<ImmutableList<AccountTrade>> GetAccountTrades(string symbol)
        {
            return GetOrCreateUserObject(
                () => _cache.GetAccountTrades(symbol),
                () => InitializeAccountTrades(symbol),
                (trades) => _cache.SetAccountTrades(symbol, trades),
                () => SubscribeUserData());
        }

        private Task<ImmutableList<Candlestick>> GetCandlestick(string symbol, CandlestickInterval interval)
        {
            return GetOrCreateUserObject(
                () => _cache.GetCandlesticks(symbol, interval),
                () => InitializeCandleticks(symbol, interval),
                (candlesticke) => _cache.SetCandlestick(symbol, interval, candlesticke),
                () => SubscribeCandlestick(symbol, interval));
        }

        private async Task<decimal> GetSymbolPrice(string symbol)
        {
            if (_cache.GetSymbols() == null)
            {
                await InitializeSymbolPrices();
            }

            SubscribeSymbols();

            return _cache.GetSymbolPrice(symbol);
        }

        private async Task<List<SymbolPrice>> GetSymbolPrices()
        {
            if(_cache.GetSymbols() == null)
            {
                await InitializeSymbolPrices();
            }

            SubscribeSymbols();

            var symbols = _cache.GetSymbols();

            var prices = new List<SymbolPrice>();

            foreach (var symbol in symbols)
            {
                var price = await GetPriceAsync(symbol);

                prices.Add(price);
            }

            return prices; 
        }

        private async Task<T> GetOrCreateUserObject<T>(Func<T> getFromCache, 
            Func<Task<T>> initialize, 
            Action<T> saveToCache,
            Action subscribe)
            where T : class
        {
            if (getFromCache() == null)
            {
                var @object = await initialize();

                saveToCache(@object);
            }

            subscribe();

            return getFromCache();
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

        private async Task<AccountInfo> InitializeAccountInfo()
        {
            using (var user = new BinanceApiUser(_config.Key, _config.Secret))
            {
                return await _binanceApi.GetAccountInfoAsync(user);
            }
        }

        private async Task<ImmutableList<Order>> InitializeOpenOrders(string symbol)
        {
            using (var user = new BinanceApiUser(_config.Key, _config.Secret))
            {
                var openOrders = await _binanceApi.GetOpenOrdersAsync(user, symbol);

                return openOrders.ToImmutableList();
            }
        }

        private async Task<ImmutableList<AccountTrade>> InitializeAccountTrades(string symbol)
        {
            using (var user = new BinanceApiUser(_config.Key, _config.Secret))
            {
                var accountTrades = await _binanceApi.GetAccountTradesAsync(user, symbol);

                return accountTrades.ToImmutableList();
            }
        }

        private async Task<ImmutableList<Candlestick>> InitializeCandleticks(string symbol, CandlestickInterval interval)
        {
            var candleticks = await _binanceApi.GetCandlesticksAsync(symbol, interval);

            return candleticks.ToImmutableList();
        }

        private async Task InitializeSymbolPrices()
        {
            var symbolPrices = await _binanceApi.GetPricesAsync();

            var symbols = new List<string>();            

            foreach (var symbolPrice in symbolPrices)
            {
                _cache.SetSymbolPrice(symbolPrice.Symbol, symbolPrice.Value);

                symbols.Add(symbolPrice.Symbol);
            }

            _cache.SetSymbols(symbols);
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

            foreach (var statistic in statistics)
            {
                _cache.SetSymbolPrice(statistic.Symbol, statistic.LastPrice);
                _cache.SetSymbolStatistic(statistic.Symbol, statistic);
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
