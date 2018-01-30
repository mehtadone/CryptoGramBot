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

        void ClearAccountInfo();

        ImmutableList<Order> GetOrders(string symbol);

        void SetOrders(string symbol, ImmutableList<Order> orders);

        void ClearOrders(string symbol);

        ImmutableList<AccountTrade> GetAccountTrades(string symbol);

        void SetAccountTrades(string symbol, ImmutableList<AccountTrade> trades);

        void ClearAccountTrades(string symbol);

        List<Symbol> GetSymbols();

        void SetSymbols(List<Symbol> symbols);

        ImmutableDictionary<string, decimal> GetSymbolPrices();

        void SetSymbolPrices(ImmutableDictionary<string, decimal> prices);

        void ClearSymbolPrices();
        
        decimal? GetSymbolPrice(string symbol);

        void ClearSymbolPrice(string symbol);

        void SetSymbolPrice(string symbol, decimal? value);

        ImmutableDictionary<string, SymbolStatistics> GetSymbolStatistics();

        void SetSymbolStatistics(ImmutableDictionary<string, SymbolStatistics> statistics);

        void ClearSymbolStatistics();

        ImmutableList<Candlestick> GetCandlesticks(string symbol, CandlestickInterval interval);

        void SetCandlestick(string symbol, CandlestickInterval interval, ImmutableList<Candlestick> candlestick);

        void ClearCandlestick(string symbol, CandlestickInterval interval);
    }
}
