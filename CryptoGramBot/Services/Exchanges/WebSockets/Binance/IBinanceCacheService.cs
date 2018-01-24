using Binance.Account;
using Binance.Account.Orders;
using Binance.Market;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace CryptoGramBot.Services.Exchanges.WebSockets.Binance
{
    public interface IBinanceCacheService
    {
        AccountInfo GetAccountInfo();

        void SetAccountInfo(AccountInfo accountInfo);

        ImmutableList<Order> GetOrders(string symbol);

        void SetOrders(string symbol, ImmutableList<Order> orders);

        ImmutableList<AccountTrade> GetAccountTrades(string symbol);

        void SetAccountTrades(string symbol, ImmutableList<AccountTrade> trades);

        List<string> GetSymbols();

        void SetSymbols(List<string> symbolPrices);

        decimal GetSymbolPrice(string symbol);

        void SetSymbolPrice(string symbol, decimal price);

        SymbolStatistics GetSymbolStatistic(string symbol);

        void SetSymbolStatistic(string symbol, SymbolStatistics statistics);

        ImmutableList<Candlestick> GetCandlesticks(string symbol, CandlestickInterval interval);

        void SetCandlestick(string symbol, CandlestickInterval interval, ImmutableList<Candlestick> candlestick);


    }
}
