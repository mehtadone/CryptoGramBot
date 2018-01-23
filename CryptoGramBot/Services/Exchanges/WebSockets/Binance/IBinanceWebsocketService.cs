using Binance.Account;
using Binance.Account.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoGramBot.Services.Exchanges.WebSockets.Binance
{
    public interface IBinanceWebsocketService
    {
        Task<AccountInfo> GetAccountInfoAsync();

        Task<IEnumerable<Order>> GetOpenOrdersAsync(string symbol);
    }
}
