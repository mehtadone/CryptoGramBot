using CryptoGramBot.EventBus;
using Enexure.MicroBus;

namespace CryptoGramBot.Extensions
{
    public static class MessageBusExtension
    {
        public static BusBuilder ConfigureCore(this BusBuilder busBuilder)
        {
            busBuilder.RegisterCommandHandler<SendMessageCommand, SendMessageHandler>();
            return busBuilder;
        }
    }
}