using Binance;
using Binance.Account;
using Binance.Account.Orders;
using Binance.Market;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace CryptoGramBot.Services.Exchanges.WebSockets.Binance
{
    public class BinanceCacheService : IBinanceCacheService
    {
        #region Consts

        private readonly string ACCOUNT_INFO_KEY = "account_info";
        private readonly string ORDERS_BY_SYMBOL_KEY = "_orders";
        private readonly string ACCOUNT_TRADES_BY_SYMBOL_KEY = "_accountTrades";
        private readonly string SYMBOL_PRICES_KEY = "symbols_prices";
        private readonly string SYMBOLS = "symbols";
        private readonly string SYMBOL_CANDLESTICK = "_candlesTick_";
        private readonly string SYMBOL_STATISTICS = "statistics";

        private readonly int CACHE_TIME_IN_MINUTES = 60;
        private readonly int EVERY_SECOND_UPDATED_ITEM_CACHE_TIME_IN_MINUTES = 1;

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

        public List<Symbol> GetSymbols()
        {
            return _memoryCache.Get<List<Symbol>>(SYMBOLS);
        }

        public void SetSymbols(List<Symbol> symbols)
        {
            _memoryCache.Set(SYMBOLS, symbols, TimeSpan.FromMinutes(CACHE_TIME_IN_MINUTES));
        }

        public ImmutableList<Candlestick> GetCandlesticks(string symbol, CandlestickInterval interval)
        {
            return _memoryCache.Get<ImmutableList<Candlestick>>($"{symbol}{SYMBOL_CANDLESTICK}{interval.AsString()}");
        }

        public void SetCandlestick(string symbol, CandlestickInterval interval, ImmutableList<Candlestick> candlesticks)
        {
            _memoryCache.Set($"{symbol}{SYMBOL_CANDLESTICK}{interval.AsString()}", candlesticks, TimeSpan.FromMinutes(EVERY_SECOND_UPDATED_ITEM_CACHE_TIME_IN_MINUTES));
        }

        public ImmutableDictionary<string, decimal> GetSymbolPrices()
        {
            return _memoryCache.Get<ImmutableDictionary<string, decimal>>(SYMBOL_PRICES_KEY);
        }

        public void SetSymbolPrices(ImmutableDictionary<string, decimal> prices)
        {
            _memoryCache.Set(SYMBOL_PRICES_KEY, prices, TimeSpan.FromMinutes(EVERY_SECOND_UPDATED_ITEM_CACHE_TIME_IN_MINUTES));
        }

        public ImmutableDictionary<string, SymbolStatistics> GetSymbolStatistics()
        {
            return _memoryCache.Get<ImmutableDictionary<string, SymbolStatistics>>(SYMBOL_STATISTICS);
        }

        public void SetSymbolStatistics(ImmutableDictionary<string, SymbolStatistics> statistics)
        {
            _memoryCache.Set(SYMBOL_STATISTICS, statistics, TimeSpan.FromMinutes(EVERY_SECOND_UPDATED_ITEM_CACHE_TIME_IN_MINUTES));
        }

        #endregion
    }
}
