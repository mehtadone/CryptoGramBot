using Enexure.MicroBus;

namespace CryptoGramBot.EventBus.Events
{
    public class BalanceCheckEvent : IEvent
    {
        public BalanceCheckEvent(bool userRequested, string exchange = null, int? coinigyAccountId = null)
        {
            UserRequested = userRequested;
            Exchange = exchange;
            CoinigyAccountId = coinigyAccountId;
        }

        public int? CoinigyAccountId { get; }
        public string Exchange { get; }
        public bool UserRequested { get; }
    }
}