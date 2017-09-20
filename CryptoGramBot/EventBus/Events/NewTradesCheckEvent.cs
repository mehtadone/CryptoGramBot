using Enexure.MicroBus;

namespace CryptoGramBot.EventBus.Events
{
    public class NewTradesCheckEvent : IEvent
    {
        public NewTradesCheckEvent(bool isStartup, bool bittrexTradeNotifcations, bool poloniexTradeNotification)
        {
            IsStartup = isStartup;
            BittrexTradeNotifcations = bittrexTradeNotifcations;
            PoloniexTradeNotification = poloniexTradeNotification;
        }

        public bool BittrexTradeNotifcations { get; }
        public bool IsStartup { get; }
        public bool PoloniexTradeNotification { get; }
    }
}