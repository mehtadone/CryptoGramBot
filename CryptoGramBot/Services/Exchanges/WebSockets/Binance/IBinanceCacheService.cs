using Binance.Account;
using Binance.Account.Orders;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace CryptoGramBot.Services.Exchanges.WebSockets.Binance
{
    public interface IBinanceCacheService
    {
        AccountInfo GetAccountInfo();

        void SetAccountInfo(AccountInfo accountInfo);

        List<Order> GetOrders(string symbol);

        void SetOrders(string symbol, List<Order> orders);

        List<AccountTrade> GetAccountTrades(string symbol);

        void SetAccountTrades(string symbol, List<AccountTrade> trades);

        ConcurrentDictionary<string, decimal> GetSymbolPrices();

        void SetSymbolPrices(ConcurrentDictionary<string, decimal> symbolPrices);

        decimal GetSymbolPrice(string symbol);

        void SetSymbolPrice(string symbol, decimal price);
    }
}
