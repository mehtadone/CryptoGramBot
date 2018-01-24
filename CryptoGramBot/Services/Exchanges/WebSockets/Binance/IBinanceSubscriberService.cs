using Binance.Api.WebSocket.Events;
using Binance.Market;
using System;
using System.Threading.Tasks;

namespace CryptoGramBot.Services.Exchanges.WebSockets.Binance
{
    public interface IBinanceSubscriberService : IDisposable
    {
        Task SymbolsStatistics(Action<SymbolStatisticsEventArgs> onUpdate);

        Task UserData(Action<OrderUpdateEventArgs> onOrderUpdate, 
            Action<AccountUpdateEventArgs> onAccountUpdate, 
            Action<AccountTradeUpdateEventArgs> onAccountTradeUpdate);

        Task Candlestick(string symbol, CandlestickInterval interval, Action<CandlestickEventArgs> onUpdate);
    }
}
