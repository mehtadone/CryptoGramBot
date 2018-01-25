using Binance;
using Binance.Account;
using Binance.Account.Orders;
using Binance.Market;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CryptoGramBot.Services.Exchanges.WebSockets.Binance
{
    public interface IBinanceWebsocketService
    {
        Task<AccountInfo> GetAccountInfoAsync();

        Task<IEnumerable<Order>> GetOpenOrdersAsync(string symbol);

        Task<IEnumerable<AccountTrade>> GetAccountTradesAsync(string symbol);

        Task<IEnumerable<Symbol>> GetSymbolsAsync();

        Task<SymbolPrice> GetPriceAsync(string symbol);

        Task<IEnumerable<SymbolPrice>> GetPricesAsync();

        Task<IEnumerable<Candlestick>> GetCandlestickAsync(string symbol, CandlestickInterval interval);

        Task<IEnumerable<SymbolStatistics>> Get24HourStatisticsAsync();
    }
}
