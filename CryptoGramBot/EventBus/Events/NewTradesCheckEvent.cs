using Enexure.MicroBus;

namespace CryptoGramBot.EventBus.Events
{
    public class NewTradesCheckEvent : IEvent
    {
        public NewTradesCheckEvent(bool isStartup)
        {
            IsStartup = isStartup;
        }

        public bool IsStartup { get; }
    }
}