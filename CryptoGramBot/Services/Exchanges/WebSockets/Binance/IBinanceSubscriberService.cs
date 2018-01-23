using Binance.Api.WebSocket.Events;
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
    }
}
