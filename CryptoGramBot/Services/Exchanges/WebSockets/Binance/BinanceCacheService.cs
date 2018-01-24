using Binance.Account;
using Binance.Account.Orders;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace CryptoGramBot.Services.Exchanges.WebSockets.Binance
{
    public class BinanceCacheService : IBinanceCacheService
    {
        #region Consts

        private readonly string ACCOUNT_INFO_KEY = "account_info";
        private readonly string ORDERS_BY_SYMBOL_KEY = "_orders";
        private readonly string ACCOUNT_TRADES_BY_SYMBOL_KEY = "_accountTrades";
        private readonly string SYMBOL_PRICES_KEY = "symbols";
        private readonly string SYMBOL_PRICE_KEY = "_price";

        private readonly int CACHE_TIME_IN_MINUTES = 60;

        #endregion

        #region Dependecies

        private readonly IMemoryCache _memoryCache;

        #endregion

        #region Constructor

        public BinanceCacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        } 

        #endregion

        #region IBinanceWebsocketCacheService

        public AccountInfo GetAccountInfo()
        {
            return _memoryCache.Get<AccountInfo>(ACCOUNT_INFO_KEY);
        }

        public void SetAccountInfo(AccountInfo accountInfo)
        {
            _memoryCache.Set(ACCOUNT_INFO_KEY, accountInfo, TimeSpan.FromMinutes(CACHE_TIME_IN_MINUTES));
        }

        public ImmutableList<Order> GetOrders(string symbol)
        {
            return _memoryCache.Get<ImmutableList<Order>>($"{symbol}{ORDERS_BY_SYMBOL_KEY}");
        }

        public void SetOrders(string symbol, ImmutableList<Order> orders)
        {
            _memoryCache.Set($"{symbol}{ORDERS_BY_SYMBOL_KEY}", orders, TimeSpan.FromMinutes(CACHE_TIME_IN_MINUTES));
        }

        public ImmutableList<AccountTrade> GetAccountTrades(string symbol)
        {
            return _memoryCache.Get<ImmutableList<AccountTrade>>($"{symbol}{ACCOUNT_TRADES_BY_SYMBOL_KEY}");
        }

        public void SetAccountTrades(string symbol, ImmutableList<AccountTrade> trades)
        {
            _memoryCache.Set($"{symbol}{ACCOUNT_TRADES_BY_SYMBOL_KEY}", trades, TimeSpan.FromMinutes(CACHE_TIME_IN_MINUTES));
        }

        public ConcurrentDictionary<string, decimal> GetSymbolPrices()
        {
            return _memoryCache.Get<ConcurrentDictionary<string, decimal>>(SYMBOL_PRICES_KEY);
        }

        public void SetSymbolPrices(ConcurrentDictionary<string, decimal> symbolPrices)
        {
            _memoryCache.Set(SYMBOL_PRICES_KEY, symbolPrices, TimeSpan.FromMinutes(CACHE_TIME_IN_MINUTES));
        }

        public decimal GetSymbolPrice(string symbol)
        {
            return _memoryCache.Get<decimal>($"{symbol}{SYMBOL_PRICE_KEY}");
        }

        public void SetSymbolPrice(string symbol, decimal price)
        {
            _memoryCache.Set($"{symbol}{SYMBOL_PRICE_KEY}", price, TimeSpan.FromMinutes(CACHE_TIME_IN_MINUTES));
        }

        #endregion
    }
}
