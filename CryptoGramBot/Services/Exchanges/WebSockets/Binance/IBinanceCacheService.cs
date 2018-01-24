using Binance.Account;
using Binance.Account.Orders;
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

        ConcurrentDictionary<string, decimal> GetSymbolPrices();

        void SetSymbolPrices(ConcurrentDictionary<string, decimal> symbolPrices);

        decimal GetSymbolPrice(string symbol);

        void SetSymbolPrice(string symbol, decimal price);
    }
}
