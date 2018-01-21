using Binance.Api.WebSocket.Events;
using Binance.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoGramBot.Services.Exchanges.WebSockets.Binance.Events
{
    public class ManySymbolStatisticsEventArgs : WebSocketClientEventArgs
    {
        public SymbolStatistics[] Statistics { get; private set; }

        public ManySymbolStatisticsEventArgs(long timestamp, CancellationToken token, SymbolStatistics[] statistics) : base(timestamp, token)
        {
            Statistics = statistics;
        }
    }
}
