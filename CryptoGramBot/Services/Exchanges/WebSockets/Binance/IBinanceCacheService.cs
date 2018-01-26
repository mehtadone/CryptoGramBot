using Binance;
using Binance.Account;
using Binance.Account.Orders;
using Binance.Market;
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

        List<Symbol> GetSymbols();

        void SetSymbols(List<Symbol> symbols);

        ImmutableDictionary<string, decimal> GetSymbolPrices();

        void SetSymbolPrices(ImmutableDictionary<string, decimal> prices);

        ImmutableDictionary<string, SymbolStatistics> GetSymbolStatistics();

        void SetSymbolStatistics(ImmutableDictionary<string, SymbolStatistics> statistics);

        ImmutableList<Candlestick> GetCandlesticks(string symbol, CandlestickInterval interval);

        void SetCandlestick(string symbol, CandlestickInterval interval, ImmutableList<Candlestick> candlestick);
    }
}
