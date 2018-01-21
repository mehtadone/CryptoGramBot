using Binance.Account;
using Binance.Account.Orders;
using Binance.Api;
using Binance.Api.WebSocket;
using Binance.Api.WebSocket.Events;
using Binance.Cache;
using Binance.Market;
using CryptoGramBot.Configuration;
using CryptoGramBot.Services.Cache;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoGramBot.Services.Exchanges.WebSockets.Binance
{
    public class BinanceWebsocketService : IDisposable
    {
        #region Fields

        private bool isDisposed;

        private readonly string ACCOUNT_INFO_KEY = "account_info";
        private readonly string ORDERS_BY_SYMBOL_KEY = "_orders";
        private readonly string ACCOUNT_TRADES_BY_SYMBOL_KEY = "_accountTrades";
        private readonly string SYMBOL_PRICES_KEY = "symbols";
        private readonly int CACHE_TIME_IN_MINUTES = 60;

        private readonly CancellationTokenSource _userDataCancellationTokenSource;
        private readonly CancellationTokenSource _symbolStatisticCancellationTokenSource;

        private readonly BinanceApiUser _user;

        private Task userDataSubscribeTask;
        private Task _symbolsSubscribeTask;

        #endregion

        #region WebSocket Clients

        private AllSymbolStatisticsWebSocketClient _allSymbolStatisticsWebSocketClient;
        private UserDataWebSocketClient _userDataWebSocketClient;

        #endregion

        #region Dependecies

        private readonly BinanceConfig _config;
        private readonly IBinanceApi _binanceApi;
        private readonly MemoryCacheService _memoryCacheService;
        private readonly IWebSocketClient _webSocketClient;
        private readonly ILogger<SymbolStatisticsWebSocketClient> _symbolStattisticsLogger;
        private readonly ILogger<UserDataWebSocketClient> _userDataWebSocketClientLogger;

        #endregion

        #region Constructor

        public BinanceWebsocketService(BinanceConfig config,
           IBinanceApi binanceApi,
           MemoryCacheService memoryCacheService,
           IWebSocketClient webSocketClient, 
           ILogger<SymbolStatisticsWebSocketClient> symbolStattisticsLogger,
           ILogger<UserDataWebSocketClient> userDataWebSocketClientLogger)
        {
            _config = config;
            _binanceApi = binanceApi;
            _memoryCacheService = memoryCacheService;
            _webSocketClient = webSocketClient;
            _symbolStattisticsLogger = symbolStattisticsLogger;
            _userDataWebSocketClientLogger = userDataWebSocketClientLogger;

            _userDataCancellationTokenSource = new CancellationTokenSource();
            _symbolStatisticCancellationTokenSource = new CancellationTokenSource();

            _user = new BinanceApiUser(_config.Key, _config.Secret);
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

            return orders.Where(p => p.Status == OrderStatus.New).ToList();
        }

        public async Task<IEnumerable<AccountTrade>> GetAccountTradesAsync(string symbol)
        {
            return await GetAccountTrades(symbol);
        }

        public async Task<IEnumerable<SymbolPrice>> GetPricesAsync()
        {
            var symbolPrices = await GetSymbolPrices();

            var prices = symbolPrices.Select(p => new SymbolPrice(p.Key, p.Value)).ToList();

            return prices;
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (!isDisposed)
            {
                _user?.Dispose();
                _symbolStatisticCancellationTokenSource?.Dispose();
                _userDataCancellationTokenSource?.Dispose();

                isDisposed = true;
            }
        }

        #endregion

        #region Methods

        private Task<AccountInfo> GetAccountInfo()
        {
            return GetOrCreateObject(
                GetAccountInfoCache, InitializeAccountInfo, SetAccountInfoCache);
        }

        private Task<List<Order>> GetOrders(string symbol)
        {
            return GetOrCreateObject(
                () => GetOrdersCache(symbol),
                () => InitializeOpenOrders(symbol),
                (orders) => SetOrdersCache(symbol, orders));
        }

        private Task<List<AccountTrade>> GetAccountTrades(string symbol)
        {
            return GetOrCreateObject(
                () => GetAccountTradesCache(symbol),
                () => InitializeAccountTrades(symbol),
                (trades) => SetAccountTradesCache(symbol, trades));
        }

        private Task<ConcurrentDictionary<string, decimal>> GetSymbolPrices()
        {
            return GetOrCreateObject(
                GetSymbolPricesCache, InitializeSymbolPrices, SetSymbolPricesCache);
        }

        private async Task<T> GetOrCreateObject<T>(Func<T> getFromCache, Func<Task<T>> initialize, Action<T> saveToCache)
            where T : class
        {
            if (getFromCache() == null)
            {
                var @object = await initialize();

                saveToCache(@object);
            }

            return getFromCache();
        }

        private AccountInfo GetAccountInfoCache()
        {
            return _memoryCacheService.Get<AccountInfo>(ACCOUNT_INFO_KEY);
        }

        private void SetAccountInfoCache(AccountInfo accountInfo)
        {
            _memoryCacheService.Set(ACCOUNT_INFO_KEY, accountInfo, CACHE_TIME_IN_MINUTES);
        }

        private List<Order> GetOrdersCache(string symbol)
        {
            return _memoryCacheService.Get<List<Order>>($"{symbol}{ORDERS_BY_SYMBOL_KEY}");
        }

        private void SetOrdersCache(string symbol, List<Order> orders)
        {
            _memoryCacheService.Set($"{symbol}{ORDERS_BY_SYMBOL_KEY}", orders, CACHE_TIME_IN_MINUTES);
        }

        private List<AccountTrade> GetAccountTradesCache(string symbol)
        {
            return _memoryCacheService.Get<List<AccountTrade>>($"{symbol}{ACCOUNT_TRADES_BY_SYMBOL_KEY}");
        }

        private void SetAccountTradesCache(string symbol, List<AccountTrade> trades)
        {
            _memoryCacheService.Set($"{symbol}{ACCOUNT_TRADES_BY_SYMBOL_KEY}", trades, CACHE_TIME_IN_MINUTES);
        }

        private ConcurrentDictionary<string, decimal> GetSymbolPricesCache()
        {
            return _memoryCacheService.Get<ConcurrentDictionary<string, decimal>>(SYMBOL_PRICES_KEY);
        }

        private void SetSymbolPricesCache(ConcurrentDictionary<string, decimal> symbolPrices)
        {
            _memoryCacheService.Set(SYMBOL_PRICES_KEY, symbolPrices, CACHE_TIME_IN_MINUTES);
        }

        private async Task<AccountInfo> InitializeAccountInfo()
        {
            using (var user = new BinanceApiUser(_config.Key, _config.Secret))
            {
                return await _binanceApi.GetAccountInfoAsync(user);
            }
        }

        private async Task<List<Order>> InitializeOpenOrders(string symbol)
        {
            using (var user = new BinanceApiUser(_config.Key, _config.Secret))
            {
                var openOrders = await _binanceApi.GetOpenOrdersAsync(user, symbol);

                return openOrders.ToList();
            }
        }

        private async Task<List<AccountTrade>> InitializeAccountTrades(string symbol)
        {
            using (var user = new BinanceApiUser(_config.Key, _config.Secret))
            {
                var accountTrades = await _binanceApi.GetAccountTradesAsync(user, symbol);

                return accountTrades.ToList();
            }
        }

        private async Task<ConcurrentDictionary<string, decimal>> InitializeSymbolPrices()
        {
            var symbolPrices = await _binanceApi.GetPricesAsync();

            var symbolPriceDictionary = new ConcurrentDictionary<string, decimal>();

            foreach (var symbolPrice in symbolPrices)
            {
                symbolPriceDictionary[symbolPrice.Symbol] = symbolPrice.Value;
            }

            return symbolPriceDictionary;
        }

        #region Subscribings

        private void SubscribeSymbolWebSocket()
        {
            if (_allSymbolStatisticsWebSocketClient == null)
            {
                try
                {
                    _allSymbolStatisticsWebSocketClient = new AllSymbolStatisticsWebSocketClient(_webSocketClient, _symbolStattisticsLogger);

                    _allSymbolStatisticsWebSocketClient.ManyStatisticsUpdate += OnManyStatisticsUpdate;

                    _symbolsSubscribeTask = _allSymbolStatisticsWebSocketClient.SubscribeAsync(e => { }, _symbolStatisticCancellationTokenSource.Token);
                }
                catch (Exception)
                { }
            }
        }

        private void SubscribeUserDataWebSocket()
        {
            if (_userDataWebSocketClient == null)
            {
                try
                {
                    _userDataWebSocketClient = new UserDataWebSocketClient(_binanceApi, _webSocketClient, null, _userDataWebSocketClientLogger);

                    _userDataWebSocketClient.TradeUpdate += OnTradeUpdate;
                    _userDataWebSocketClient.AccountUpdate += OnAccountUpdate;
                    _userDataWebSocketClient.OrderUpdate += OnOrderUpdate;

                    userDataSubscribeTask = _userDataWebSocketClient.SubscribeAsync(_user, _userDataCancellationTokenSource.Token);
                }
                catch (Exception)
                { }
            }
        } 

        #endregion

        #region EventHandlers Methods

        private void OnTradeUpdate(object sender, AccountTradeUpdateEventArgs e)
        {
            var symbol = e.Trade.Symbol;
            var accountTrades = GetAccountTradesCache(symbol).ToList();

            if (accountTrades != null)
            {
                var previosTrade = accountTrades.FirstOrDefault(p => p.Id == e.Trade.Id);

                if (previosTrade != null)
                {
                    accountTrades.Remove(previosTrade);
                }

                accountTrades.Add(e.Trade);

                SetAccountTradesCache(symbol, accountTrades);
            }
        }

        private void OnAccountUpdate(object sender, AccountUpdateEventArgs e)
        {
            SetAccountInfoCache(e.AccountInfo);
        }

        private void OnOrderUpdate(object sender, OrderUpdateEventArgs args)
        {
            var symbol = args.Order.Symbol;

            var orders = GetOrdersCache(symbol).ToList();

            if (orders != null)
            {
                var previosOrder = orders.FirstOrDefault(p => args.Order.Id == p.Id);

                if (previosOrder != null)
                {
                    orders.Remove(previosOrder);
                }

                orders.Add(args.Order);

                SetOrdersCache(symbol, orders);
            }
        }

        private void OnManyStatisticsUpdate(object sender, Events.ManySymbolStatisticsEventArgs e)
        {
            var prices = GetSymbolPricesCache();

            if (prices != null)
            {
                var statistics = e.Statistics;

                foreach (var statistic in statistics)
                {
                    prices[statistic.Symbol] = statistic.LastPrice;
                }
            }
        } 

        #endregion

        #endregion
    }
}
