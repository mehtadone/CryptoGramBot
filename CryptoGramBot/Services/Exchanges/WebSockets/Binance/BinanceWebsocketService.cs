using Binance.Account;
using Binance.Account.Orders;
using Binance.Api;
using Binance.Api.WebSocket.Events;
using Binance.Market;
using CryptoGramBot.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
            var orders = await GetOrders(symbol);

            return orders.Where(order => order.IsOpen());
        }

        public async Task<IEnumerable<AccountTrade>> GetAccountTradesAsync(string symbol)
        {
            return await GetAccountTrades(symbol);
        }

        public async Task<SymbolPrice> GetPrice(string symbol)
        {
            var symbolPrice = await GetSymbolPrice(symbol);

            return new SymbolPrice(symbol, symbolPrice);
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
                (accountInfo) => _cache.SetAccountInfo(accountInfo));
        }

        private Task<List<Order>> GetOrders(string symbol)
        {
            return GetOrCreateUserObject(
                () => _cache.GetOrders(symbol),
                () => InitializeOpenOrders(symbol),
                (orders) => _cache.SetOrders(symbol, orders));
        }

        private Task<List<AccountTrade>> GetAccountTrades(string symbol)
        {
            return GetOrCreateUserObject(
                () => _cache.GetAccountTrades(symbol),
                () => InitializeAccountTrades(symbol),
                (trades) => _cache.SetAccountTrades(symbol, trades));
        }

        private async Task<decimal> GetSymbolPrice(string symbol)
        {
            if (_cache.GetSymbolPrices() == null)
            {
                await InitializeSymbolPrices();
            }

            SubscribeSymbols();

            return _cache.GetSymbolPrice(symbol);
        }

        private async Task<T> GetOrCreateUserObject<T>(Func<T> getFromCache, Func<Task<T>> initialize, Action<T> saveToCache)
            where T : class
        {
            if (getFromCache() == null)
            {
                var @object = await initialize();

                saveToCache(@object);
            }

            SubscribeUserData();

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

        #endregion

        #region Initial initialization

        private async Task<AccountInfo> InitializeAccountInfo()
        {
            using (var user = new BinanceApiUser(_config.Key, _config.Secret))
            {
                return await _binanceApi.GetAccountInfoAsync(user, 10000000);
            }
        }

        private async Task<List<Order>> InitializeOpenOrders(string symbol)
        {
            using (var user = new BinanceApiUser(_config.Key, _config.Secret))
            {
                var openOrders = await _binanceApi.GetOpenOrdersAsync(user, symbol, 10000000);

                return openOrders.ToList();
            }
        }

        private async Task<List<AccountTrade>> InitializeAccountTrades(string symbol)
        {
            using (var user = new BinanceApiUser(_config.Key, _config.Secret))
            {
                var accountTrades = await _binanceApi.GetAccountTradesAsync(user, symbol, -1L, 0, 10000000);

                return accountTrades.ToList();
            }
        }

        private async Task InitializeSymbolPrices()
        {
            var symbolPrices = await _binanceApi.GetPricesAsync();

            _cache.SetSymbolPrices(new ConcurrentDictionary<string, decimal>());

            foreach (var symbolPrice in symbolPrices)
            {
                _cache.SetSymbolPrice(symbolPrice.Symbol, symbolPrice.Value);
            }
        }

        #endregion

        #region Event Handlers Methods

        private void OnAccountTradeUpdate(AccountTradeUpdateEventArgs e)
        {
            var symbol = e.Trade.Symbol;
            var accountTrades = _cache.GetAccountTrades(symbol).ToList();

            if (accountTrades != null)
            {
                var previousTrade = accountTrades.FirstOrDefault(p => p.Id == e.Trade.Id);

                if (previousTrade != null)
                {
                    accountTrades.Remove(previousTrade);
                }

                accountTrades.Add(e.Trade);

                _cache.SetAccountTrades(symbol, accountTrades);

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
            var orders = _cache.GetOrders(symbol).ToList();

            if(orders != null)
            {
                var previousOrder = orders.FirstOrDefault(p => updatedOrder.Id == p.Id);

                if (previousOrder != null)
                {
                    orders.Remove(previousOrder);
                }

                orders.Add(updatedOrder);

                _cache.SetOrders(symbol, orders);
            }
        }

        private void OnStatisticsUpdate(SymbolStatisticsEventArgs e)
        {
            var statistics = e.Statistics;

            foreach (var statistic in statistics)
            {
                _cache.SetSymbolPrice(statistic.Symbol, statistic.LastPrice);
            }
        }

        #endregion

        #endregion
    }
}
