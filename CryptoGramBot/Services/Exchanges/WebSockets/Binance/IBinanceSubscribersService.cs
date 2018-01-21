using Binance.Api.WebSocket.Events;
using System;

namespace CryptoGramBot.Services.Exchanges.WebSockets.Binance
{
    public interface IBinanceSubscribersService : IDisposable
    {
        void AddSymbols(Action<SymbolStatisticsEventArgs> onUpdate);

        void AddUserData(Action<OrderUpdateEventArgs> onOrderUpdate, 
            Action<AccountUpdateEventArgs> onAccountUpdate, 
            Action<AccountTradeUpdateEventArgs> onAccountTradeUpdate);
    }
}
