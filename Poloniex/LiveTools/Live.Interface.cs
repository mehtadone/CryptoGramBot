using System;
using System.Threading.Tasks;

namespace Jojatekok.PoloniexAPI
{
    public interface ILive
    {
        /// <summary>Occurs when a currency pair's market data changes.</summary>
        event EventHandler<TickerChangedEventArgs> OnTickerChanged;
        /// <summary>Occurs when someone sends a message on the trollbox.</summary>
        event EventHandler<TrollboxMessageEventArgs> OnTrollboxMessage;

        /// <summary>Initializes the live feed.</summary>
        void Start();
        /// <summary>Disposes the live feed.</summary>
        void Stop();

        /// <summary>Starts the process of receiving price ticker messages.</summary>
        Task SubscribeToTickerAsync();
        /// <summary>Starts the process of receiving trollbox messages.</summary>
        Task SubscribeToTrollboxAsync();
    }
}
