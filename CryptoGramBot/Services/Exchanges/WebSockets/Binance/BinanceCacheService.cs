using Binance;
using Binance.Account;
using Binance.Account.Orders;
using Binance.Market;
using Microsoft.Extensions.Caching.Memory;
using System;
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
        private readonly string SYMBOL_PRICE = "price_";
        
        private readonly int CACHE_TIME_IN_MINUTES = 60;
        private readonly int USER_CACHE_TIME_IN_HOURS = 23;

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
            _memoryCache.Set(ACCOUNT_INFO_KEY, accountInfo, TimeSpan.FromHours(USER_CACHE_TIME_IN_HOURS));
        }

        public ImmutableList<Order> GetOrders(string symbol)
        {
            return _memoryCache.Get<ImmutableList<Order>>($"{symbol}{ORDERS_BY_SYMBOL_KEY}");
        }

        public void SetOrders(string symbol, ImmutableList<Order> orders)
        {
            _memoryCache.Set($"{symbol}{ORDERS_BY_SYMBOL_KEY}", orders, TimeSpan.FromHours(USER_CACHE_TIME_IN_HOURS));
        }

        public ImmutableList<AccountTrade> GetAccountTrades(string symbol)
        {
            return _memoryCache.Get<ImmutableList<AccountTrade>>($"{symbol}{ACCOUNT_TRADES_BY_SYMBOL_KEY}");
        }

        public void SetAccountTrades(string symbol, ImmutableList<AccountTrade> trades)
        {
            _memoryCache.Set($"{symbol}{ACCOUNT_TRADES_BY_SYMBOL_KEY}", trades, TimeSpan.FromHours(USER_CACHE_TIME_IN_HOURS));
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
            _memoryCache.Set($"{symbol}{SYMBOL_CANDLESTICK}{interval.AsString()}", candlesticks, TimeSpan.FromMinutes(CACHE_TIME_IN_MINUTES));
        }

        public ImmutableDictionary<string, decimal> GetSymbolPrices()
        {
            return _memoryCache.Get<ImmutableDictionary<string, decimal>>(SYMBOL_PRICES_KEY);
        }

        public void SetSymbolPrices(ImmutableDictionary<string, decimal> prices)
        {
            _memoryCache.Set(SYMBOL_PRICES_KEY, prices, TimeSpan.FromMinutes(CACHE_TIME_IN_MINUTES));
        }

        public ImmutableDictionary<string, SymbolStatistics> GetSymbolStatistics()
        {
            return _memoryCache.Get<ImmutableDictionary<string, SymbolStatistics>>(SYMBOL_STATISTICS);
        }

        public void SetSymbolStatistics(ImmutableDictionary<string, SymbolStatistics> statistics)
        {
            _memoryCache.Set(SYMBOL_STATISTICS, statistics, TimeSpan.FromMinutes(CACHE_TIME_IN_MINUTES));
        }

        public void ClearAccountInfo()
        {
            _memoryCache.Remove(ACCOUNT_INFO_KEY);
        }

        public void ClearOrders(string symbol)
        {
            _memoryCache.Remove($"{symbol}{ORDERS_BY_SYMBOL_KEY}");
        }

        public void ClearAccountTrades(string symbol)
        {
            _memoryCache.Remove($"{symbol}{ACCOUNT_TRADES_BY_SYMBOL_KEY}");
        }

        public void ClearSymbolPrices()
        {
            _memoryCache.Remove(SYMBOL_PRICES_KEY);
        }

        public void ClearSymbolStatistics()
        {
            _memoryCache.Remove(SYMBOL_STATISTICS);
        }

        public void ClearCandlestick(string symbol, CandlestickInterval interval)
        {
            _memoryCache.Remove($"{symbol}{SYMBOL_CANDLESTICK}{interval.AsString()}");
        }

        public decimal? GetSymbolPrice(string symbol)
        {
            return _memoryCache.Get<decimal?>($"{SYMBOL_PRICE}{symbol}");
        }

        public void ClearSymbolPrice(string symbol)
        {
            _memoryCache.Remove($"{SYMBOL_PRICE}{symbol}");
        }

        public void SetSymbolPrice(string symbol, decimal? value)
        {
            _memoryCache.Set($"{SYMBOL_PRICE}{symbol}", value, TimeSpan.FromMinutes(CACHE_TIME_IN_MINUTES));
        }



        #endregion
    }
}
